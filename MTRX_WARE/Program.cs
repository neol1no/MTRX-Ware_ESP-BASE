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
Entity currentEntity = new Entity();


// offsets

// offsets.cs | 
int dwEntityList = 0x1A1F730;
int dwViewMatrix = 0x1A89130;
int dwLocalPlayerPawn = 0x1874050;

// client_dll.cs | 
int m_vOldOrigin = 0x1324;
int m_iTeamNum = 0x3E3;
int m_lifeState = 0x348;
int m_hPlayerPawn = 0x814;
int m_vecViewOffset = 0xCB0;
int m_iszPlayerName = 0x660;
int m_modelState = 0x170;
int m_pGameSceneNode = 0x328;
int m_entitySpottedState = 0x11A8;
int m_bSpotted = 0x8;

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
    localPlayer.team = swed.ReadInt(localPlayerPawn, m_iTeamNum);

    // entity list loop
    for (int i = 0; i < 64; i++)
    {
        // Get controller
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);
        if (currentController == IntPtr.Zero) continue;

        // Get pawn handle
        int pawnHandle = swed.ReadInt(currentController, m_hPlayerPawn);
        if (pawnHandle == 0) continue;

        // Get pawn + make second entry
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
        if (listEntry2 == IntPtr.Zero) continue;

        // Get current pawn
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));
        if (currentPawn == IntPtr.Zero) continue;

        // Life check
        int lifeState = swed.ReadInt(currentPawn, m_lifeState);
        if (lifeState != 256) continue;

        // Get matrix
        float[] viewMatrix = swed.ReadMatrix(client + dwViewMatrix);

        IntPtr sceneNode = swed.ReadPointer(currentPawn, m_pGameSceneNode);
        IntPtr boneMatrix = swed.ReadPointer(sceneNode, m_modelState + 0x80);

        // Populate entity
        Entity entity = new Entity();

        entity.name = swed.ReadString(currentController, m_iszPlayerName, 16).Split("\0")[0];
        entity.team = swed.ReadInt(currentPawn, m_iTeamNum);
        entity.spotted = swed.ReadBool(currentPawn, m_entitySpottedState + m_bSpotted);
        entity.position = swed.ReadVec(currentPawn, m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn, m_vecViewOffset);
        entity.position2D = Calculate.WorldToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WorldToScreen(viewMatrix, Vector3.Add(entity.position, entity.viewOffset), screenSize);

        // Get local player position
        localPlayer.position = swed.ReadVec(localPlayerPawn, m_vOldOrigin);

        // Update distance for local player
        foreach (var otherEntity in entities)
        {
            otherEntity.distance = Vector3.Distance(otherEntity.position, localPlayer.position);
        }


        // Read bone data
        entity.bones = Calculate.ReadBones(boneMatrix, swed);
        entity.bones2d = Calculate.ReadBones2d(entity.bones, viewMatrix, screenSize);

        // Add the entity to the entities list
        entities.Add(entity);
    }


    // Update renderer data (pass the entities and localPlayer)
    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);

    // Sleep for a short time before next iteration (to avoid overloading the CPU)
    // Thread.Sleep(1);
}
