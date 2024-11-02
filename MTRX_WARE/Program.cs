using MTRX_WARE;
using Swed64;
using System.Numerics;

// main logic

// init swed
Swed swed = new Swed("cs2");

// get client module 
IntPtr client = swed.GetModuleBase("client.dll");

// init render
Renderer renderer = new Renderer();
Thread renderThread = new Thread(new ThreadStart(renderer.Start().Wait));
renderThread.Start();

// get screen size
Vector2 screenSize = renderer.screenSize;

// store entities
List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();



// offsets

// offsets.cs | update every game update
int dwEntityList = 0x19D1A98;
int dwViewMatrix = 0x1A33E30;
int dwLocalPlayerPawn = 0x1836BB8;

// client_dll.cs | cometimes update
int m_vOldOrigin = 0x1324;
int m_iTeamNum = 0x3E3;
int m_lifeState = 0x348;
int m_hPlayerPawn = 0x80C;
int m_vecViewOffset = 0xCB0;
int m_iszPlayerName = 0x660;

// ESP loop
while (true)
{
    entities.Clear();

    // get entity list
    IntPtr entityList = swed.ReadPointer(client, dwEntityList);

    // make entry
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    // get localplayer
    IntPtr localPlayerPawn = swed.ReadPointer(client, dwLocalPlayerPawn);

    // get team
    localPlayer.team = swed.ReadInt(localPlayerPawn, m_iTeamNum);

    // entity list loop
    for (int i = 0; i < 64; i++)
    {
        // get controller
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);

        if (currentController == IntPtr.Zero) continue;

        // get pawn handle
        int pawnHandle = swed.ReadInt(currentController, m_hPlayerPawn);
        if (pawnHandle == 0) continue;

        // get pawn + make second entry
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
        if (listEntry2 == IntPtr.Zero) continue;

        // get current pawn
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));
        if (currentPawn == IntPtr.Zero) continue;

        // lifecheck
        int lifeState = swed.ReadInt(currentPawn, m_lifeState);
        if (lifeState != 256) continue;

        // get matrix
        float[] viewMatrix = swed.ReadMatrix(client + dwViewMatrix);

        // populate entity
        Entity entity = new Entity();

        entity.name = swed.ReadString(currentController, m_iszPlayerName, 16).Split("\0")[0];
        entity.team = swed.ReadInt(currentPawn, m_iTeamNum);
        entity.position = swed.ReadVec(currentPawn, m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn, m_vecViewOffset);
        entity.position2D = Calculate.WorldToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WorldToScreen(viewMatrix, Vector3.Add(entity.position, entity.viewOffset), screenSize);

        entities.Add(entity);
    }

    // update renderer data
    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);

    // Thread

}