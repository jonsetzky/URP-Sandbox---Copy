# Pixelate

An Unity URP renderer feature which pixelates objects.
Based on [Madalaski's Pixel Art shader tutorial](https://github.com/Madalaski/PixelatedAdvancedTutorial/tree/master)

# Deployment

1. Import package
   - from git https://github.com/jonsetzky/unity-pixelated.git
   - or locally from disk (this can be done to develop the package)
2. Add the `Pixelate` renderer feature to your renderer asset.
3. **Remove the generated pixelate layer from renderer asset's opaque and transparent layer masks.**
4. Attach `Pixelated/Pixelate` component to an object to pixelate it.

# TODO

It works only on opaque forward shaders. Add support for others, such as transparent.

Add support for HDRP.

# Problems

Pixelated resolution could be something weird. I haven't looked into it. When investigating this look into the RTHandles.Alloc function as it takes different scaling options as arguments.

Changing pixel density scales the texture in weird way. The scaling gets fixed
after entering/exiting play mode.

SSAO doesn't work with pixelated objects. The fix is probably in render order.
The layer has to be changed to pixelated before opaque objects are rendered and
back to normal after pixel pass.

# Notes

Causes a memory leak on 2022.3.9f1. [Due to this?](https://forum.unity.com/threads/rthandles-api-introduced-catastrophic-memory-leak-bug-in-2022-3-8.1486035/)
