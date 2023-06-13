using Vintagestory.API.Common;

[assembly: ModInfo("Joy of Painting")]

namespace jopainting
{
    public class JOPaintingMod : ModSystem
    {
        public static ICoreAPI coreApi;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("BlockPainting", typeof(BlockPainting));
            api.RegisterBlockEntityClass("BlockEntityPainting", typeof(BlockEntityPainting));
            api.RegisterBlockBehaviorClass("PaintingAttachable", typeof(BlockBehaviorPaintingAttachable));
        }
    }
}