using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace jopainting
{
    public class BlockPainting : BlockContainer, IContainedMeshSource
    {
        Dictionary<int, MeshRef> meshrefs = new();

        protected virtual string MeshRefsCacheKey => Code.ToShortString() + "meshRefs";
        static ModSystemPainting paintingModSys;

        public int CurrentMeshRefid => GetHashCode();

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropChanceMultiplier)
        {
            ItemStack[] droppedItemstack = base.GetDrops(world, pos, byPlayer, dropChanceMultiplier);
            if (droppedItemstack == null) return droppedItemstack;
            if (world.BlockAccessor.GetBlockEntity(pos) is not BlockEntityPainting bep) return droppedItemstack;

            foreach (ItemStack stack in droppedItemstack)
            {
                stack.Attributes.SetInt("width", bep.width);
                stack.Attributes.SetInt("height", bep.height);
                stack.Attributes.SetString("paintingR", bep.paintingR);
                stack.Attributes.SetString("paintingG", bep.paintingG);
                stack.Attributes.SetString("paintingB", bep.paintingB);
                stack.Attributes.SetString("paintingname", bep.name);
            }

            return droppedItemstack;
        }

        public override void OnLoaded(ICoreAPI api)
        {
            paintingModSys = api.ModLoader.GetModSystem<ModSystemPainting>();
            base.OnLoaded(api);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            ItemStack pickedItemstack =  base.OnPickBlock(world, pos);
            if (pickedItemstack == null) return pickedItemstack;
            pickedItemstack = new ItemStack(world.GetBlock(pickedItemstack.Collectible.Code));

            if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityPainting bep)
            {
                pickedItemstack.Attributes.SetInt("width", bep.width);
                pickedItemstack.Attributes.SetInt("height", bep.height);
                pickedItemstack.Attributes.SetString("paintingR", bep.paintingR);
                pickedItemstack.Attributes.SetString("paintingG", bep.paintingG);
                pickedItemstack.Attributes.SetString("paintingB", bep.paintingB);
                pickedItemstack.Attributes.SetString("paintingname", bep.name);
            }
            return pickedItemstack;
        }

        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            if (capi.ObjectCache.TryGetValue(MeshRefsCacheKey, out object obj))
            {
                meshrefs = obj as Dictionary<int, MeshRef>;
            }
            else
            {
                capi.ObjectCache[MeshRefsCacheKey] = meshrefs = new Dictionary<int, MeshRef>();
            }

            var meshrefid = itemstack.Attributes.GetInt("meshRefId");
            if (meshrefid == 0 || !meshrefs.TryGetValue(meshrefid, out renderinfo.ModelRef))
            {
                var num = meshrefs.Count + 1;
                var value = capi.Render.UploadMesh(GenMesh(itemstack, capi.BlockTextureAtlas, null));
                renderinfo.ModelRef = meshrefs[num] = value;
                itemstack.Attributes.SetInt("meshRefId", num);
            }
        }

        MeshData origcontainermesh;

        public MeshData GenMesh(ICoreClientAPI capi, ItemStack itemstack, BlockPos forBlockPos = null)
        {
            if (origcontainermesh == null)
            {
                capi.Tesselator.TesselateShape(this, capi.TesselatorManager.GetCachedShape(this.Shape.Base), out origcontainermesh, new Vec3f(Shape.rotateX, Shape.rotateY, Shape.rotateZ));
            }

            MeshData containerMesh = origcontainermesh.Clone();

            AssetLocation photoBlock = new((this.Attributes?["paintingshape"]?.AsString("jopainting:paintingrenderer") ?? "jopainting:paintingrenderer") + "-" + this.LastCodePart());

            Block block = capi.World.GetBlock(photoBlock);
            if (block == null) return containerMesh;
            MeshData photoMesh = capi.TesselatorManager.GetDefaultBlockMesh(block);

            string paintingR = itemstack.Attributes.GetString("paintingR", "");
            string paintingG = itemstack.Attributes.GetString("paintingG", "");
            string paintingB = itemstack.Attributes.GetString("paintingB", "");
            int width = itemstack.Attributes.GetInt("width", 0);
            int height = itemstack.Attributes.GetInt("height", 0);

            if (paintingR?.Length == 0 || paintingG?.Length == 0 || paintingB?.Length == 0) return containerMesh;

            Bitmap bmpR = BitmapUtil.GrayscaleBitmapFromPixels(Encoding.GetEncoding(28591).GetBytes(paintingR), width, height);
            Bitmap bmpG = BitmapUtil.GrayscaleBitmapFromPixels(Encoding.GetEncoding(28591).GetBytes(paintingG), width, height);
            Bitmap bmpB = BitmapUtil.GrayscaleBitmapFromPixels(Encoding.GetEncoding(28591).GetBytes(paintingB), width, height);

            PaintingBitmap bitmap = new();

            bmpR = new Bitmap(bmpR, new Size(32, 32));
            bmpG = new Bitmap(bmpG, new Size(32, 32));
            bmpB = new Bitmap(bmpB, new Size(32, 32));

            bitmap.SetBitmapRGB(bmpR, bmpG, bmpB);

            TextureAtlasPosition atlasPosition = paintingModSys.GetAtlasPosition(bitmap, capi, paintingR + paintingG + paintingB);

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

            containerMesh.AddMeshData(photoMesh);

            return containerMesh;
        }

        public override string GetHeldItemName(ItemStack itemStack)
        {
            string paintingname = itemStack.Attributes.GetString("paintingname", "");
            if (paintingname != "") return base.GetHeldItemName(itemStack) + " (" + paintingname + ")";
            return base.GetHeldItemName(itemStack);
        }

        public MeshData GenMesh(ItemStack itemstack, ITextureAtlasAPI targetAtlas, BlockPos atBlockPos)
        {
            return GenMesh(api as ICoreClientAPI, itemstack, atBlockPos);
        }

        public string GetMeshCacheKey(ItemStack itemstack)
        {
            string s = MeshRefsCacheKey;
            string R = itemstack?.Attributes?.GetString("paintingR");
            string G = itemstack?.Attributes?.GetString("paintingG");
            string B = itemstack?.Attributes?.GetString("paintingB");
            return s + R + G + B;
        }
    }
}
