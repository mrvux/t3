using System;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_2dc5c9d1_ea93_4597_a4d9_7b610aad603a
{
    public class MixPoints : Instance<MixPoints>
    {

        [Output(Guid = "660013c7-8f6b-458a-bb86-61e5a85692a4")]
        public readonly Slot<T3.Core.DataTypes.BufferWithViews> OutBuffer = new();

        [Input(Guid = "ba7ffda2-f9f6-440d-a174-7339844835fa")]
        public readonly InputSlot<float> BlendValue = new InputSlot<float>();

        [Input(Guid = "c5480ce5-a8ba-4a26-8cee-c28e442020b7", MappedType = typeof(BlendModes))]
        public readonly InputSlot<int> BlendMode = new InputSlot<int>();

        [Input(Guid = "ACEF877D-214D-4CA0-AC11-95FA59D1F6FC", MappedType = typeof(PairingModes))]
        public readonly InputSlot<int> Pairing = new();


        [Input(Guid = "97904d2e-ae67-4ab4-9201-7902a85d12f3")]
        public readonly InputSlot<T3.Core.DataTypes.BufferWithViews> PointsA_ = new InputSlot<T3.Core.DataTypes.BufferWithViews>();

        [Input(Guid = "91b903a2-5127-431b-ab66-d5a38ce1693c")]
        public readonly InputSlot<T3.Core.DataTypes.BufferWithViews> PointsB_ = new InputSlot<T3.Core.DataTypes.BufferWithViews>();

        private enum BlendModes
        {
            Blend,
            UseW1AsWeight,
            UseW2AsWeight
        }
        
        private enum PairingModes
        {
            WrapAround,
            Adjust,
        }
    }
} 

