# Pixelate

An Unity URP renderer feature which pixelates objects.
Based on [Madalaski's Pixel Art shader tutorial](https://github.com/Madalaski/PixelatedAdvancedTutorial/tree/master)

# Deployment

1. Create a Pixelated layer.
2. Add the `Pixelate` renderer feature to your renderer asset. And make sure that:
   - It's layer is the pixelate layer
   - RP Event is `Before Rendering Transparents`
3. Remove the pixelate layer from renderer asset's opaque and transparent layer masks.
4. Now every object with a forward rendering shader on the pixelate layer should be pixelated.

# TODO

It works only on opaque forward shaders. Add support for others, such as transparent.

Add support for HDRP.

The feature has a pixel density slider but it doesn't update immediately. Fix that.

# Problems

Pixelated objects cannot be layered since their layer has to be a specific one.

Pixelated resolution could be something weird. I haven't looked into it. When investigating this look into the RTHandles.Alloc function as it takes different scaling options as arguments.

# Notes

Causes a memory leak on 2022.3.9f1. [Due to this?](https://forum.unity.com/threads/rthandles-api-introduced-catastrophic-memory-leak-bug-in-2022-3-8.1486035/)
