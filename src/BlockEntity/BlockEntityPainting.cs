using System.Drawing;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace jopainting
{
    public class BlockEntityPainting : BlockEntity
    {
        public PaintingBitmap bitmap;
        TextureAtlasPosition atlasPosition;
        LoadedTexture loadedTex;
        int texSubId = 0;

        ModSystemPainting paintingModSys;

        public int imageArraySize = -1;

        public string desc = "";

        bool isPhotoUpdated = false;

        bool wasJustPlaced = false;

        public AssetLocation photoBlock = new("jopainting", "paintingrenderer");
        MeshData photoMesh;

        #region Attributes
        public string paintingR = "";
        public string paintingG = "";
        public string paintingB = "";
        public int width = 0;
        public int height = 0;
        public string name = "";
        #endregion

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            paintingModSys = api.ModLoader.GetModSystem<ModSystemPainting>();

            photoBlock = new AssetLocation((Block.Attributes?["paintingshape"]?.AsString("jopainting:paintingrenderer") ?? "jopainting:paintingrenderer") + "-" + Block.LastCodePart());

            GenPhoto();
            MarkDirty(true);
        }

        public BlockEntityPainting()
        {
            bitmap = new PaintingBitmap();
        }

        public override void OnBlockPlaced(ItemStack byItemStack)
        {
            if (byItemStack == null)
            {
                MarkDirty(true);

                return;
            }

            if (byItemStack.Attributes.GetInt("height", 0) == 0)
            {
                base.OnBlockPlaced(byItemStack);

                GenPhoto();
            }

            wasJustPlaced = true;

            paintingR = byItemStack.Attributes.GetString("paintingR", "");
            paintingG = byItemStack.Attributes.GetString("paintingG", "");
            paintingB = byItemStack.Attributes.GetString("paintingB", "");
            width = byItemStack.Attributes.GetInt("width", 0);
            height = byItemStack.Attributes.GetInt("height", 0);
            name = byItemStack.Attributes.GetString("paintingname", "");

            base.OnBlockPlaced(byItemStack);

            GenPhoto();
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            if (wasJustPlaced)
            {
                tree.SetString("paintingR", paintingR);
                tree.SetString("paintingG", paintingG);
                tree.SetString("paintingB", paintingB);
                tree.SetInt("width", width);
                tree.SetInt("height", height);
                tree.SetString("paintingname", name);

                wasJustPlaced = false;
            }

            paintingR = tree.GetString("paintingR", "");
            paintingG = tree.GetString("paintingG", "");
            paintingB = tree.GetString("paintingB", "");
            width = tree.GetInt("width", 0);
            height = tree.GetInt("height", 0);
            name = tree.GetString("paintingname", "");

            GenPhoto();
            base.FromTreeAttributes(tree, worldAccessForResolve);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            tree.SetString("paintingR", paintingR);
            tree.SetString("paintingG", paintingG);
            tree.SetString("paintingB", paintingB);
            tree.SetInt("width", width);
            tree.SetInt("height", height);
            tree.SetString("paintingname", name);
            base.ToTreeAttributes(tree);
        }

        private void GenPhoto()
        {
            if (Api == null) return;
            if (Api.Side != EnumAppSide.Client) return;

            if (paintingR?.Length == 0 || paintingG?.Length == 0 || paintingB?.Length == 0) return;

            ICoreClientAPI capi = Api as ICoreClientAPI;

            Bitmap bmpR = BitmapUtil.GrayscaleBitmapFromPixels(Encoding.GetEncoding(28591).GetBytes(paintingR), width, height);
            Bitmap bmpG = BitmapUtil.GrayscaleBitmapFromPixels(Encoding.GetEncoding(28591).GetBytes(paintingG), width, height);
            Bitmap bmpB = BitmapUtil.GrayscaleBitmapFromPixels(Encoding.GetEncoding(28591).GetBytes(paintingB), width, height);

            bmpR = new Bitmap(bmpR, new Size(32, 32));
            bmpG = new Bitmap(bmpG, new Size(32, 32));
            bmpB = new Bitmap(bmpB, new Size(32, 32));

            bitmap.SetBitmapRGB(bmpR, bmpG, bmpB);

            atlasPosition = paintingModSys.GetAtlasPosition(bitmap, capi, paintingR + paintingG + paintingB);

            LoadMesh();
        }

        private void LoadMesh()
        {
            if (Api.Side == EnumAppSide.Server) return;

            photoBlock = new AssetLocation((Block.Attributes?["paintingshape"]?.AsString("jopainting:paintingrenderer") ?? "jopainting:paintingrenderer") + "-" + Block.LastCodePart());

            Block block = Api.World.GetBlock(photoBlock);
            if (block == null) return;
            ICoreClientAPI capi = Api as ICoreClientAPI;
            photoMesh = capi.TesselatorManager.GetDefaultBlockMesh(block);

            if (atlasPosition != null)
            {
                photoMesh = photoMesh.WithTexPos(atlasPosition);

                photoMesh.Uv[6] = atlasPosition.x1;
                photoMesh.Uv[7] = atlasPosition.y1;

                photoMesh.Uv[4] = atlasPosition.x2;
                photoMesh.Uv[5] = atlasPosition.y1;

                photoMesh.Uv[2] = atlasPosition.x2;
                photoMesh.Uv[3] = atlasPosition.y2;

                photoMesh.Uv[0] = atlasPosition.x1;
                photoMesh.Uv[1] = atlasPosition.y2;
            }
            isPhotoUpdated = true;
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (isPhotoUpdated) mesher.AddMeshData(photoMesh);

            return base.OnTesselation(mesher, tessThreadTesselator);
        }
    }
}
