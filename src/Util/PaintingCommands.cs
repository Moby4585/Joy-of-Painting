using System.Drawing;
using System.Windows.Forms;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace jopainting
{
    class PaintingCommands : ModSystem
    {
        ModSystemPainting paintingModSys;

        public enum ImageType { File, Url, Clipboard }

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

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            var parsers = api.ChatCommands.Parsers;
            api.ChatCommands.Create("renameimg")
                .WithDescription(Lang.Get("jopainting:Description.RenameHeld"))
                .RequiresPrivilege(Privilege.chat)
                .WithArgs(parsers.All("name"))
                .HandleWith(RenamePainting);
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

        public TextCommandResult RenamePainting(TextCommandCallingArgs args)
        {
            var activeSlot = args?.Caller?.Player?.InventoryManager?.ActiveHotbarSlot;

            if (activeSlot.Empty)
            {
                return TextCommandResult.Error("jopainting:Error.NotHoldingRequired");
            }
            if (!activeSlot.Itemstack.ItemAttributes.IsTrue("isPainting"))
            {
                return TextCommandResult.Error(Lang.Get("jopainting:Error.NotHoldingRequired"));
            }

            activeSlot.Itemstack.Attributes.SetString("paintingname", args[0].ToString());
            activeSlot.MarkDirty();

            return TextCommandResult.Success(Lang.Get("jopainting:Success.Renamed", "placeholder"));
        }
    }
}
