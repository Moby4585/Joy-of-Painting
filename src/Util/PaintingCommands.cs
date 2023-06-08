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
using System.Windows.Forms;

namespace jopainting
{
    class PaintingCommands : ModSystem
    {
        ModSystemPainting paintingModSys;

        public enum ImageType { File, Url, Clipboard }

        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            paintingModSys = api.ModLoader.GetModSystem<ModSystemPainting>();

            var parsers = api.ChatCommands.Parsers;
            api.ChatCommands.Create("loadimg")
            .BeginSubCommand("file")
                .WithArgs(parsers.All("file"))
                .HandleWith(x => LoadPainting(x, ImageType.File))
            .EndSubCommand()
            .BeginSubCommand("url")
                .WithArgs(parsers.All("url"))
                .HandleWith(x => LoadPainting(x, ImageType.Url))
            .EndSubCommand()
            .BeginSubCommand("cb")
                .HandleWith(x => LoadPainting(x, ImageType.Clipboard))
            .EndSubCommand();
        }

        public TextCommandResult LoadPainting(TextCommandCallingArgs args, ImageType type)
        {
            if (args.Caller.Player.InventoryManager.ActiveHotbarSlot.Empty)
            {
                return TextCommandResult.Error("jopainting:Error.NotHoldingRequired");
            }
            if (!args.Caller.Player.InventoryManager.ActiveHotbarSlot.Itemstack.ItemAttributes.IsTrue("isPainting"))
            {
                return TextCommandResult.Error(Lang.Get("jopainting:Error.NotHoldingRequired"));
            }

            PaintingBitmap bitmap = new();

            Bitmap bmp = null;

            switch (type)
            {
                case ImageType.File:
                    {
                        bmp = ModSystemPainting.LoadBmpFromFile(args[0].ToString());
                        if (bmp?.Width == 1) return TextCommandResult.Error(Lang.Get("jopainting:Error.Clipboard.FileNotFound", args[0].ToString()));
                        break;
                    }
                case ImageType.Url:
                    {
                        bmp = ModSystemPainting.LoadBmpFromUrl(args[0].ToString());
                        if (bmp?.Width == 1) return TextCommandResult.Error(Lang.Get("jopainting:Error.Clipboard.UrlNotFound"));
                        break;
                    }
                case ImageType.Clipboard:
                    {
                        if (!Clipboard.ContainsImage()) return TextCommandResult.Error(Lang.Get("jopainting:Error.Clipboard.NoImage"));
                        bmp = ModSystemPainting.LoadBmpFromClipboard();
                        if (bmp?.Width == 1) return TextCommandResult.Error(Lang.Get("jopainting:Error.Clipboard.NoImage"));
                        break;
                    }
            }

            bmp = new Bitmap(bmp, new Size(32, 32));

            bitmap.SetBitmap(bmp);

            paintingModSys.SavePainting(args.Caller.Player, bitmap.pixelsRed, bitmap.pixelsGreen, bitmap.pixelsBlue, bitmap.Width, bitmap.Height, "placeholder");

            return TextCommandResult.Success(Lang.Get("jopainting:Success.RequestLoad", "placeholder"));
        }
    }
}
