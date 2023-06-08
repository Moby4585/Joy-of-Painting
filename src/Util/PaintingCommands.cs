using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;
using Vintagestory.Client;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using OpenTK.Graphics;
using ProtoBuf;

namespace jopainting
{
    class PaintingCommands : ModSystem
    {
        ModSystemPainting paintingModSys;

        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            paintingModSys = api.ModLoader.GetModSystem<ModSystemPainting>();

            var parsers = api.ChatCommands.Parsers;
            api.ChatCommands.Create("loadpainting")
                .WithArgs(parsers.All("file"))
                .HandleWith(LoadPainting);
        }

        public TextCommandResult LoadPainting(TextCommandCallingArgs args)
        {
            if (args.Caller.Player.InventoryManager.ActiveHotbarSlot.Empty)
            {
                return TextCommandResult.Error("Error: you have to be holding a painting (from Joy of Painting, not vanilla) to use this command");
            }
            if (args.Caller.Player.InventoryManager.ActiveHotbarSlot.Itemstack.ItemAttributes.IsTrue("isPainting") != true)
            {
                return TextCommandResult.Error("Error: you have to be holding a painting (from Joy of Painting, not vanilla) to use this command");
            }


            PaintingBitmap bitmap = new PaintingBitmap();
            Bitmap bmp = ModSystemPainting.loadBmp(args[0].ToString());

            if (bmp.Width == 1) return TextCommandResult.Error("Error: File \"" + args[0].ToString() + ".bmp\" not found in VintagestoryData/Paintings folder");

            bmp = new Bitmap(bmp, new Size(32, 32));

            bitmap.setBitmap(bmp);

            paintingModSys.savePainting(args.Caller.Player, bitmap.pixelsRed, bitmap.pixelsGreen, bitmap.pixelsBlue, bitmap.Width, bitmap.Height, args[0].ToString());

            return TextCommandResult.Success("Requested loading \"" + args[0] + "\"");
        }
    }
}
