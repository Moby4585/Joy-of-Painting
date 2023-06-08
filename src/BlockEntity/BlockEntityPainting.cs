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

        // Attributes
        public string paintingR = "";
        public string paintingG = "";
        public string paintingB = "";
        public int width = 0;
        public int height = 0;
        public string name = "";

        //public ItemStack fromStack = null;

        // Inventory : 0 - liquide, 1 - solide, 2 - lampe

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            RegisterGameTickListener(OnGameTick, 5);

            //apparatusComposition = GetApparatusComposition();
            paintingModSys = api.ModLoader.GetModSystem<ModSystemPainting>();

            photoBlock = new AssetLocation((Block.Attributes?["paintingshape"]?.AsString("jopainting:paintingrenderer") ?? "jopainting:paintingrenderer") + "-" + Block.LastCodePart());

            GenPhoto();
            //((BlockPainting)this.Block).be = this;
            //((BlockPainting)this.Block).setValues();
            MarkDirty(true);
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            //Api.World.SpawnItemEntity(fromStack, this.Pos.ToVec3d());

            base.OnBlockBroken(byPlayer);
        }

        public BlockEntityPainting()
        {
            bitmap = new PaintingBitmap();
        }

        public void OnGameTick(float dt)
        {
        }

        public bool OnInteract(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, bool onlyOilLamp = false)
        {
            MarkDirty(true);

            return false;
        }

        public override void OnBlockPlaced(ItemStack byItemStack)
        {
            if (byItemStack == null)
            {
                //base.OnBlockPlaced(byItemStack);

                MarkDirty(true);

                return;
            }

            if (byItemStack.Attributes.GetInt("height", 0) == 0)
            {
                //fromStack = byItemStack.Clone();
                //fromStack.StackSize = 1;

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


            //fromStack = byItemStack.Clone();
            //fromStack.StackSize = 1;

            base.OnBlockPlaced(byItemStack);

            GenPhoto();
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {

            //reactingRecipe = JsonUtil.FromString<RetortRecipe>(tree.GetString("reactingRecipe", ""));
            if (wasJustPlaced)
            {
                tree.SetString("paintingR", paintingR);
                tree.SetString("paintingG", paintingG);
                tree.SetString("paintingB", paintingB); ;
                tree.SetInt("width", width);
                tree.SetInt("height", height);
                tree.SetString("paintingname", name);

                //tree.SetItemstack("fromstack", fromStack);
                wasJustPlaced = false;
            }

            paintingR = tree.GetString("paintingR", "");
            paintingG = tree.GetString("paintingG", "");
            paintingB = tree.GetString("paintingB", "");
            width = tree.GetInt("width", 0);
            height = tree.GetInt("height", 0);
            name = tree.GetString("paintingname", "");

            GenPhoto();
            //fromStack = tree.GetItemstack("fromstack", null);

            //((BlockPainting)this.Block).be = this;
            //((BlockPainting)this.Block).setValues();

            base.FromTreeAttributes(tree, worldAccessForResolve);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            //tree.SetString("reactingRecipe", JsonUtil.ToString(reactingRecipe));
            tree.SetString("paintingR", paintingR);
            tree.SetString("paintingG", paintingG);
            tree.SetString("paintingB", paintingB); ;
            tree.SetInt("width", width);
            tree.SetInt("height", height);
            tree.SetString("paintingname", name);

            //tree.SetItemstack("fromstack", fromStack);

            //((BlockPainting)this.Block).be = this;
            //((BlockPainting)this.Block).setValues();

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

            //var graphics = Graphics.FromImage(bmp);
            //graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            //graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            //bmp = new Bitmap(bmp, new Size(bmp.Width / (bmp.Height / (int)Math.Pow(2, JoPaintingConfig.Current.PhotographLod)), (int)Math.Pow(2, JoPaintingConfig.Current.PhotographLod)));
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
