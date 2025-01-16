using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Threading;

namespace MTRX_WARE
{
    public class Renderer : Overlay
    {
        // render variables
        public Vector2 screenSize = new Vector2(1920, 1080); // own screen size

        // entities copy | more thread safe method
        private ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        private Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        // Gui elements
        private bool enableESP = true;
        private bool VisCheck = true;
        private bool enableNameESP = true;
        private bool enableTeleport = false;
        private bool enableFlip = false;
        private bool enableDesync = false;
        private bool enableExtendedKnife = false;
        private bool enableBhop = false;
        private bool enableLineESP = true;
        private bool enableBoxESP = true;
        private bool enableSkeletonESP = true;
        private bool enableDistanceESP = true;
        private Vector4 enemyColor = new Vector4(1, 0, 0, 1); // default red
        private Vector4 hiddenColor = new Vector4(0, 0, 0, 1); // default black
        private Vector4 teamColor = new Vector4(0, 1, 0, 1); // default green
        private Vector4 nameColor = new Vector4(1, 1, 1, 1); // default white
        private Vector4 boneColor = new Vector4(1, 1, 1, 1); // default white
        private Vector4 distanceColor = new Vector4(1, 1, 1, 1); // default white

        // Declare the color variables globally (outside of the render loop) to make them persistent
        Vector4 windowBgColor = new Vector4(0.2f, 0.2f, 0.2f, 0.8f);
        Vector4 boxBgColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        Vector4 headerColor = new Vector4(0.18f, 0.18f, 0.18f, 1.0f);
        Vector4 buttonColor = new Vector4(0.18f, 0.18f, 0.18f, 1.0f);
        Vector4 textColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        // Define custom colors for the checkbox
        Vector4 checkboxColor = new Vector4(0.2f, 0.8f, 0.2f, 1.0f); // Example: Green color for checkbox

        // Change checkbox color when the checkbox is not checked
        Vector4 checkboxInactiveColor = new Vector4(1.0f, 0.6f, 0.6f, 1.0f);


        float boneThickness = 20;

        // draw list
        ImDrawListPtr drawlist;

        private int switchTabs = 0;
        

        

        public void RenderMenu()
        {
            ImGui.Begin("MTRX-Ware.io");

            // Apply persistent colors at the start (once for the entire window)
            ImGui.PushStyleColor(ImGuiCol.WindowBg, windowBgColor);
            ImGui.PushStyleColor(ImGuiCol.TitleBg, headerColor);  // Apply header color
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, headerColor);  // Active header color
            ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.22f, 0.22f, 0.22f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.Text, textColor);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 10.0f);

            // Set color for the checkmark (checked state)
            ImGui.PushStyleColor(ImGuiCol.CheckMark, checkboxColor);

            // Set background color for the checkbox (unchecked state)
            ImGui.PushStyleColor(ImGuiCol.FrameBg, checkboxInactiveColor);
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.47058823529411764f, 0.47058823529411764f, 0.47058823529411764f, 0.9f)); // Hovered background for unchecked checkbox
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new Vector4(0.2f, 0.8f, 0.2f, 1.0f)); // Active background for unchecked checkbox

            // Create columns for the menu and content
            ImGui.Columns(2, "menu_columns", true); // Two columns, with the option to divide them

            // Left column (Menu buttons)
            ImGui.SetColumnWidth(0, 150);
            ImGui.BeginGroup();
            if (ImGui.Button("Aimbot", new Vector2(130.0f, 30.0f)))
                switchTabs = 0;
            ImGui.Spacing();
            if (ImGui.Button("Visuals", new Vector2(130.0f, 30.0f)))
                switchTabs = 1;
            ImGui.Spacing();
            if (ImGui.Button("Exploits", new Vector2(130.0f, 30.0f)))
                switchTabs = 2;
            ImGui.Spacing();
            if (ImGui.Button("Misc", new Vector2(130.0f, 30.0f)))
                switchTabs = 3;
            ImGui.Spacing();
            if (ImGui.Button("Menu", new Vector2(130.0f, 30.0f)))
                switchTabs = 4;
            ImGui.EndGroup();

            
            ImGui.NextColumn();

            // Right column (Content)
            ImGui.BeginGroup();

            
            Vector2 windowPos = ImGui.GetWindowPos();
            float windowX = windowPos.X;
            float windowY = windowPos.Y;
            float columnX = ImGui.GetColumnOffset(1);
            float columnWidth = ImGui.GetColumnWidth(1);

            float boxX = windowX + columnX + 5.0f;
            float boxY = windowY + ImGui.GetCursorPosY();
            float boxWidth = columnWidth - 20.0f;
            float boxHeight = ImGui.GetTextLineHeightWithSpacing() * 14;

            
            ImGui.GetWindowDrawList().AddRectFilled(
                new Vector2(boxX, boxY),
                new Vector2(boxX + boxWidth, boxY + boxHeight),
                ImGui.ColorConvertFloat4ToU32(boxBgColor),
                10.0f,
                ImDrawFlags.RoundCornersAll
            );

            
            float xOffset = 20.0f;

            switch (switchTabs)
            {
                case 0: // AIMBOT
                    break;

                case 1: // VISUALS
                        
                    float yOffset = ImGui.GetCursorPosY() + 8;



                    float horizontalOffset = 190.0f;
                    Vector2 previewSize = new Vector2(200, 200);
                    Vector2 cursorStart = ImGui.GetCursorPos();

                    ImGui.SetCursorPos(new Vector2(cursorStart.X + horizontalOffset, cursorStart.Y));
                    ImGui.Text("ESP Preview:");

                    ImGui.SetCursorPos(new Vector2(cursorStart.X + horizontalOffset, cursorStart.Y + ImGui.GetTextLineHeightWithSpacing() + 5));

                    Vector2 previewPosition = ImGui.GetCursorScreenPos();

                    ImGui.GetWindowDrawList().AddRect(previewPosition, previewPosition + previewSize, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)));

                    ImDrawListPtr previewDrawList = ImGui.GetWindowDrawList();

                    DrawESPPreview(previewDrawList, previewPosition, previewSize);

                    ImGui.Dummy(previewSize);



                    // Enable ESP checkbox
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable ESP", ref enableESP);

                    // Enable Box ESP checkbox
                    yOffset = ImGui.GetCursorPosY(); 
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable Box ESP", ref enableBoxESP);

                    // Enable Skeleton ESP checkbox
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable Skeleton ESP", ref enableSkeletonESP);

                    // Enable Line ESP checkbox
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable Line ESP", ref enableLineESP);

                    // Enable Name ESP checkbox
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable Name ESP", ref enableNameESP);

                    // Enable Distance ESP checkbox
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Enable Distance ESP", ref enableDistanceESP);

                    // Team Color text
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Team Color");

                    // Team Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.SameLine();
                    ImGui.ColorEdit4("##teamcolor", ref teamColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf);

                    // Enemy Color text
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Enemy Color");

                    // Enemy Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.SameLine();
                    ImGui.ColorEdit4("##enemycolor", ref enemyColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf);

                    // Bone Color text
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Bone Color");

                    // Bone Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.SameLine();
                    ImGui.ColorEdit4("##bonecolor", ref boneColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf);

                    // Distance Color text
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Distance Color");

                    // Distance Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.SameLine();
                    ImGui.ColorEdit4("##distancecolor", ref distanceColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf);

                    break;

                case 2: // EXPLOITS
                    break;

                case 3: // MISC
                        // Bhop checkbox
                    yOffset = ImGui.GetCursorPosY() + 8;
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Checkbox("Bhop", ref enableBhop);
                    break;

                case 4: // menu
                    // Background Color picker
                    yOffset = ImGui.GetCursorPosY() + 8;
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Background Color");
                    ImGui.SameLine();
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + 150 + xOffset, yOffset - 5));
                    if (ImGui.ColorEdit4("##bgcolor", ref windowBgColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf))
                    {
                        // Color updated
                    }

                    // Box Background Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Box Background Color");
                    ImGui.SameLine();
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + 150 + xOffset, yOffset - 5));
                    if (ImGui.ColorEdit4("##boxbgcolor", ref boxBgColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf))
                    {
                        // Color updated
                    }

                    // Header Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Header Color");
                    ImGui.SameLine();
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + 150 + xOffset, yOffset - 5));
                    if (ImGui.ColorEdit4("##headercolor", ref headerColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf))
                    {
                        // Color updated
                    }

                    // Button Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Button Color");
                    ImGui.SameLine();
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + 150 + xOffset, yOffset - 5));
                    if (ImGui.ColorEdit4("##buttoncolor", ref buttonColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf))
                    {
                        // Color updated
                    }

                    // Text Color picker
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + xOffset, yOffset));
                    ImGui.Text("Text Color");
                    ImGui.SameLine();
                    yOffset = ImGui.GetCursorPosY();
                    ImGui.SetCursorPos(new Vector2(columnX + 150 + xOffset, yOffset - 5));
                    if (ImGui.ColorEdit4("##textcolor", ref textColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaPreviewHalf))
                    {
                        // Color updated
                    }
                    break;
            }

            ImGui.EndGroup();
            ImGui.Columns(1);

            // Pop style colors after the menu rendering
            ImGui.PopStyleColor(7); // Pop 7 color settings
            ImGui.PopStyleVar();
        }










        private DateTime lastInsertPressTime = DateTime.MinValue;

        // Time delay in seconds (1 second)
        private const float keyPressDelay = 1.0f;

        // Importing the GetAsyncKeyState function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetAsyncKeyState(int vKey);

        private bool isMenuVisible = false;

        private bool IsInsertKeyPressed()
        {
            // Virtual key code for the Insert key is 0x2D
            const int VK_INSERT = 0x2D;
            short keyState = GetAsyncKeyState(VK_INSERT);

            // Check if the Insert key is pressed and if 1 second has passed since the last press
            if ((keyState & 0x8000) != 0)
            {
                // Get the current time
                DateTime currentTime = DateTime.Now;

                // Check if the key press delay has passed
                if ((currentTime - lastInsertPressTime).TotalSeconds >= keyPressDelay)
                {
                    lastInsertPressTime = currentTime;  // Update the last press time
                    return true;
                }
            }

            return false;
        }

        protected override void Render()
        {
            // Example for ImGui's key state checking:
            if (IsInsertKeyPressed())
            {
                Console.WriteLine("Insert key pressed!");
                isMenuVisible = !isMenuVisible; // Toggle menu visibility
            }

            // Render the menu if visible
            if (isMenuVisible)
            {
                RenderMenu();
            }
            else
            {
                DrawOverlay(screenSize);  // Or any other background drawing logic
            }

            drawlist = ImGui.GetWindowDrawList();

                // draw stuff
                if (enableESP)
                {
                    if (enableBoxESP)
                    {

                        foreach(var entity in entities)
                        {
                            if (EntityOnScreen(entity))
                            {
                                DrawBox(entity);
                            }
                        }
                    }
                    if (enableLineESP)
                    {
                        foreach (var entity in entities)
                        {
                            if (EntityOnScreen(entity))
                            {
                                DrawLine(entity);
                            }
                        }
                    }
                    if (enableNameESP)
                    {
                        foreach (var entity in entities)
                        {
                            if (EntityOnScreen(entity))
                            {
                                DrawName(entity, 20);
                            }
                        }
                    }
                    if (enableSkeletonESP)
                    {
                        foreach (var entity in entities)
                        {
                            if (EntityOnScreen(entity))
                            {
                                DrawBones(entity);
                            }
                        }
                    }
                   if (enableDistanceESP)
                    {
                        foreach (var entity in entities)
                        {
                            if (EntityOnScreen(entity) && entity != localPlayer)
                            {
                                entity.distance = Vector3.Distance(localPlayer.position, entity.position);
                                DrawDistance(localPlayer, entity); // Always recalculate and render
                            }
                        }
                    }
                }
        }

        private void DrawESPPreview(ImDrawListPtr previewDrawList, Vector2 previewPosition, Vector2 previewSize)
        {
            // Dummy entity setup
            List<Entity> previewEntities = new List<Entity>
    {
        new Entity { name = "Enemy1", team = 2, spotted = true, position2D = new Vector2(50, 50), viewPosition2D = new Vector2(50, 20) },
        
        new Entity { name = "Teammate1", team = 1, spotted = true, position2D = new Vector2(100, 150), viewPosition2D = new Vector2(100, 120) }
    };

            // Dummy local player for reference
            Entity dummyLocalPlayer = new Entity { position = new Vector3(0, 0, 0) };

            // Scaling factor for the preview
            float scale = previewSize.X / 200.0f;

            // Iterate through dummy entities and draw enabled ESP features
            foreach (var entity in previewEntities)
            {
                // Scale positions for preview
                Vector2 scaledPosition2D = previewPosition + (entity.position2D * scale);
                Vector2 scaledViewPosition2D = previewPosition + (entity.viewPosition2D * scale);

                // Draw ESP features based on settings
                if (enableBoxESP)
                {
                    float entityHeight = scaledPosition2D.Y - scaledViewPosition2D.Y;
                    Vector2 rectTop = new Vector2(scaledViewPosition2D.X - entityHeight / 3, scaledViewPosition2D.Y);
                    Vector2 rectBottom = new Vector2(scaledViewPosition2D.X + entityHeight / 3, scaledPosition2D.Y);
                    Vector4 boxColor = entity.team == 1 ? teamColor : enemyColor;
                    previewDrawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
                }

                if (enableNameESP)
                {
                    previewDrawList.AddText(scaledViewPosition2D - new Vector2(0, 15), ImGui.ColorConvertFloat4ToU32(nameColor), entity.name);
                }

                if (enableDistanceESP)
                {
                    float distance = Vector3.Distance(dummyLocalPlayer.position, entity.position);
                    previewDrawList.AddText(scaledPosition2D + new Vector2(0, 5), ImGui.ColorConvertFloat4ToU32(distanceColor), $"{distance:F1}m");
                }

                if (enableLineESP)
                {
                    previewDrawList.AddLine(previewPosition + new Vector2(previewSize.X / 2, previewSize.Y), scaledPosition2D, ImGui.ColorConvertFloat4ToU32(enemyColor));
                }
            }
        }

        // check position
        bool EntityOnScreen(Entity entity)
        {
            if (entity.position2D.X > 0 && entity.position2D.X < screenSize.X && entity.position2D.Y > 0 && entity.position2D.Y < screenSize.Y)
            {
                return true;
            }
            return false;
        }

        // drawing method

        private void DrawBones(Entity entity)
        {
            uint uintColor = ImGui.ColorConvertFloat4ToU32(boneColor);
            float currentBoneThickness = 1 + boneThickness / entity.distance;

            drawlist.AddLine(entity.bones2d[1], entity.bones2d[2], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[1], entity.bones2d[3], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[1], entity.bones2d[6], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[3], entity.bones2d[4], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[6], entity.bones2d[7], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[4], entity.bones2d[5], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[7], entity.bones2d[8], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[1], entity.bones2d[0], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[0], entity.bones2d[9], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[0], entity.bones2d[11], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[9], entity.bones2d[10], uintColor, currentBoneThickness);
            drawlist.AddLine(entity.bones2d[11], entity.bones2d[12], uintColor, currentBoneThickness);
            drawlist.AddCircle(entity.bones2d[2], 8 + currentBoneThickness, uintColor);
        }

        public void DrawName(Entity entity, int yOffset)
        {
            Vector2 textLocation = new Vector2(entity.viewPosition2D.X, entity.viewPosition2D.Y - yOffset);
            drawlist.AddText(textLocation, ImGui.ColorConvertFloat4ToU32(nameColor), $"{entity.name}");
        }

        public void DrawDistance(Entity localPlayer, Entity entity)
        {
            float distance = Vector3.Distance(localPlayer.position, entity.position);

            Vector2 textLocation = new Vector2(entity.viewPosition2D.X, entity.position2D.Y + 5);

            drawlist.AddText(textLocation, ImGui.ColorConvertFloat4ToU32(distanceColor), $"{distance:F1}m");
        }

        public void DrawBox(Entity entity)
        {
            // box height
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;

            // box dimensions
            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);
            Vector2 rectBottom = new Vector2(entity.viewPosition2D.X + entityHeight / 3, entity.position2D.Y);

            // color
            Vector4 boxColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            // hidden check
            if (VisCheck)
                boxColor = entity.spotted == true ? boxColor : hiddenColor;

            drawlist.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
        }
        private void DrawLine(Entity entity)
        {   
            // team color
            Vector4 lineColor = localPlayer.team == entity.team ? teamColor: enemyColor;

            // draw line
            drawlist.AddLine(new Vector2(screenSize.X / 2, screenSize.Y), entity.position2D, ImGui.ColorConvertFloat4ToU32(lineColor));
        }

        // transfer entity methods

        public void UpdateEntities(IEnumerable<Entity> newEntities)
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
        }

        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock(entityLock)
            {
                localPlayer = newEntity;
            }
        }

        public Entity GetLocalPlayer()
        {
            lock(entityLock) {
            return localPlayer;
            }
        }

        void DrawOverlay(Vector2 screenSize) // Overlay window
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }
    }
}
