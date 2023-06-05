using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using System.Drawing;
using System.IO;

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


