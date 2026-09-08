Shader "Custom/Outline Mask" {
  Properties {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
    _StencilRef("Stencil Ref", Int) = 1
_StencilWriteMask("Stencil Write Mask", Int) = 3   // was 15 — now 0x03 (bits 0-1)
_StencilReadMask("Stencil Read Mask", Int) = 3
  }
  SubShader {
    Tags {
      "Queue" = "Transparent+100"
      "RenderType" = "Transparent"
    }
    Pass {
      Name "Mask"
      Cull Off
      ZTest [_ZTest]
      ZWrite Off
      ColorMask 0
      Stencil {
        Ref [_StencilRef]
        ReadMask [_StencilReadMask]
        WriteMask [_StencilWriteMask]
        Pass Replace
      }
    }
  }
}