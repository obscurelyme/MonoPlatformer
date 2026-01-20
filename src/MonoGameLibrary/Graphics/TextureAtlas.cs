using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public class TextureAtlas
{
  private Dictionary<string, TextureRegion> _regions;

  // Stores animations added to this atlas.
  private Dictionary<string, Animation> _animations;

  /// <summary>
  /// Gets or Sets the source texture represented by this texture atlas.
  /// </summary>
  public Texture2D Texture { get; set; }


  public TextureAtlas()
  {
    _regions = new Dictionary<string, TextureRegion>();
    _animations = new Dictionary<string, Animation>();
  }

  public TextureAtlas(Texture2D texture)
  {
    Texture = texture;
    _regions = new Dictionary<string, TextureRegion>();
    _animations = new Dictionary<string, Animation>();
  }

  public void AddRegion(string name, int x, int y, int width, int height)
  {
    TextureRegion region = new TextureRegion(Texture, x, y, width, height);
    _regions.Add(name, region);
  }

  /// <summary>
  /// Adds the given animation to this texture atlas with the specified name.
  /// </summary>
  /// <param name="animationName">The name of the animation to add.</param>
  /// <param name="animation">The animation to add.</param>
  public void AddAnimation(string animationName, Animation animation)
  {
    _animations.Add(animationName, animation);
  }

  public TextureRegion GetRegion(string name)
  {
    return _regions[name];
  }

  /// <summary>
  /// Gets the animation from this texture atlas with the specified name.
  /// </summary>
  /// <param name="animationName">The name of the animation to retrieve.</param>
  /// <returns>The animation with the specified name.</returns>
  public Animation GetAnimation(string animationName)
  {
    return _animations[animationName];
  }

  public bool RemoveRegion(string name)
  {
    return _regions.Remove(name);
  }

  /// <summary>
  /// Removes the animation with the specified name from this texture atlas.
  /// </summary>
  /// <param name="animationName">The name of the animation to remove.</param>
  /// <returns>true if the animation is removed successfully; otherwise, false.</returns>
  public bool RemoveAnimation(string animationName)
  {
    return _animations.Remove(animationName);
  }

  public void Clear()
  {
    _regions.Clear();
    _animations.Clear();
  }

  /// <summary>
  /// Creates a new texture atlas based on a texture atlas xml configuration file.
  /// </summary>
  /// <param name="content">The content manager used to load the texture for the atlas.</param>
  /// <param name="fileName">The path to the xml file, relative to the content root directory.</param>
  /// <returns>The texture atlas created by this method.</returns>
  public static TextureAtlas FromFile(ContentManager manager, string fileName)
  {
    TextureAtlas atlas = new TextureAtlas();
    string filePath = Path.Combine(manager.RootDirectory, fileName);

    using (Stream stream = TitleContainer.OpenStream(filePath))
    {
      using (XmlReader reader = XmlReader.Create(stream))
      {
        XDocument doc = XDocument.Load(reader);
        XElement root = doc.Root;

        string texturePath = root.Element("Texture").Value;
        atlas.Texture = manager.Load<Texture2D>(texturePath);

        var regions = root.Element("Regions")?.Elements("Region");

        if (regions != null)
        {
          foreach (var region in regions)
          {
            string name = region.Attribute("name")?.Value;
            int x = int.Parse(region.Attribute("x")?.Value ?? "0");
            int y = int.Parse(region.Attribute("y")?.Value ?? "0");
            int width = int.Parse(region.Attribute("width")?.Value ?? "0");
            int height = int.Parse(region.Attribute("height")?.Value ?? "0");

            if (!string.IsNullOrEmpty(name))
            {
              atlas.AddRegion(name, x, y, width, height);
            }
          }
        }

        var animations = root.Element("Animations")?.Elements("Animation");

        if (animations != null)
        {
          foreach (var animation in animations)
          {
            string name = animation.Attribute("name")?.Value;
            float delayMs = float.Parse(animation.Attribute("delay")?.Value ?? "0");
            TimeSpan delay = TimeSpan.FromMilliseconds(delayMs);

            List<TextureRegion> frames = new List<TextureRegion>();
            var frameElements = animation.Elements("Frame");

            if (frameElements != null)
            {
              foreach (var frameElement in frameElements)
              {
                string textureRegionName = frameElement.Attribute("region")?.Value;
                TextureRegion region = atlas.GetRegion(textureRegionName);
                frames.Add(region);
              }
            }

            Animation anim = new Animation(frames, delay);
            atlas.AddAnimation(name, anim);
          }
        }

        return atlas;
      }
    }
  }

  public Sprite CreateSprite(string regionName)
  {
    TextureRegion region = GetRegion(regionName);
    return new Sprite(region);
  }

  public AnimatedSprite CreateAnimatedSprite(string animationName)
  {
    Animation animation = GetAnimation(animationName);
    return new AnimatedSprite(animation);
  }
}
