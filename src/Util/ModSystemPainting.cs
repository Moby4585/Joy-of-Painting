using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace jopainting
{
    class ModSystemPainting : ModSystem
    {
        public static ICoreAPI api;

        public Dictionary<string, TextureAtlasPosition> atlasPositions = new();

        public override double ExecuteOrder()
        {
            return 0.3d;
        }

        public override void Start(ICoreAPI Api)
        {
            base.Start(Api);
            api = Api;
            api.Network.RegisterChannel("savepainting").RegisterMessageType<SavePaintingPacket>();
        }

        public TextureAtlasPosition GetAtlasPosition(PaintingBitmap painting, ICoreClientAPI capi, string picture)
        {
            if (atlasPositions.ContainsKey(picture)) return atlasPositions[picture];

            capi.BlockTextureAtlas.InsertTexture(painting, out int texSubId, out TextureAtlasPosition atlasPosition);

            atlasPositions.Add(picture, atlasPosition);

            return atlasPosition;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Network.GetChannel("savepainting").SetMessageHandler<SavePaintingPacket>(OnSavePaintingPacket);
        }

        public void SavePainting(IPlayer player, byte[] paintingR, byte[] paintingG, byte[] paintingB, int width, int height, string name)
        {
            if (api is ICoreClientAPI capi)
            {
                capi.Network.GetChannel("savepainting").SendPacket(new SavePaintingPacket() { PaintingR = paintingR, PaintingG = paintingG, PaintingB = paintingB, Width = width, Height = height, Name = name });
            }
        }

        public void OnSavePaintingPacket(IServerPlayer player, SavePaintingPacket packet)
        {
            ItemStack paintingStack = player.InventoryManager.ActiveHotbarSlot.Itemstack;

            paintingStack?.Attributes.SetInt("width", packet.Width);
            paintingStack?.Attributes.SetInt("height", packet.Height);
            paintingStack?.Attributes.SetString("paintingR", Encoding.GetEncoding(28591).GetString(packet.PaintingR));
            paintingStack?.Attributes.SetString("paintingG", Encoding.GetEncoding(28591).GetString(packet.PaintingG));
            paintingStack?.Attributes.SetString("paintingB", Encoding.GetEncoding(28591).GetString(packet.PaintingB));
            paintingStack?.Attributes.SetString("paintingname", packet.Name);

            player.InventoryManager.ActiveHotbarSlot.MarkDirty();
        }

        public static Bitmap LoadBmpFromFile(string fileName)
        {
            List<string> formats = new()
            {
                ".bmp",
                ".gif",
                ".ico",
                ".jpeg",
                ".jpg",
                ".png",
                ".tiff"
            };

            foreach (var format in formats)
            {
                if (File.Exists($"{api.GetOrCreateDataPath("Paintings")}/{fileName}" + format))
                {
                    return (Bitmap)Image.FromFile($"{api.GetOrCreateDataPath("Paintings")}/{fileName}" + format);
                }
            }
            return new(1, 1);
        }

        public static Bitmap LoadBmpFromUrl(string url)
        {
            try
            {
                PictureBox picbox = new();
                picbox.Load(url);

                if (picbox?.Width == 1)
                {
                    return new(1, 1);
                }

                return (Bitmap)picbox.Image;
            }
            catch (Exception)
            {
                return new(1, 1);
            }
        }

        public static Bitmap LoadBmpFromClipboard()
        {
            var img = Clipboard.GetImage();

            if (img?.Width == 1)
            {
                return new(1, 1);
            }
            return (Bitmap)img;
        }
    }
    [ProtoContract]
    public class SavePaintingPacket
    {
        [ProtoMember(1)]
        public byte[] PaintingR;
        [ProtoMember(2)]
        public byte[] PaintingG;
        [ProtoMember(3)]
        public byte[] PaintingB;
        [ProtoMember(4)]
        public int Width;
        [ProtoMember(5)]
        public int Height;
        [ProtoMember(6)]
        public string Name;
    }
}
