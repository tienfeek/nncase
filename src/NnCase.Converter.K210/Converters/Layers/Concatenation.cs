﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NnCase.Converter.Converters;
using NnCase.Converter.K210.Converters.Stages.Convert;
using NnCase.Converter.K210.Converters.Stages.Inference;
using NnCase.Converter.Model;
using NnCase.Converter.Model.Layers;

namespace NnCase.Converter.K210.Converters.Layers
{
    [LayerConverter(typeof(Concatenation), K210LayerType.Invalid)]
    public class ConcatenationConverter
    {
        public object Convert(Concatenation layer, ConvertContext context)
        {
            foreach (var input in layer.Inputs)
            {
                (var groups, _) = K210Helper.GetRowLayout(input.Dimensions[3]);
                var channels = input.Dimensions[0];
                if (channels % groups != 0)
                    throw new LayerNotSupportedException(nameof(Concatenation), $"Channels must can be diveded by groups ({groups})");
            }

            return null;
        }

        public void AllocateInputMemory(Concatenation layer, OutputConnector input, InferenceContext context)
        {
            var totalAlloc = context.GetOrAllocateMainMemory(layer.Output);
            uint offset = 0;

            foreach (var node in layer.Inputs.Select(x => x.Connection.From))
            {
                if (context.MainMemoryMap.ContainsKey(node))
                    return;
                uint size = (uint)node.Dimensions.GetSize() * 4;
                context.MainMemoryMap.Add(node, new MemoryAllocation(totalAlloc.Node, offset, size));
                offset += size;
            }
        }
    }
}