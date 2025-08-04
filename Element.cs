using System.Numerics;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Raylib.UI;

public enum LayoutMode
{
    Horizontal,
    Vertical
}

public enum Alignment
{
    TopLeft, TopCenter, TopRight,
    CenterLeft, CenterCenter, CenterRight,
    BottomLeft, BottomCenter, BottomRight
}

public class UIElement
{
    public string Name;
    public LayoutMode Layout = LayoutMode.Horizontal;
    public Alignment ChildAlignment = Alignment.TopLeft;
    public float Gap = 0;
    public float OffsetX = 0;
    public float OffsetY = 0;
    public Vector2 Offset => new (OffsetX, OffsetY);
    public Size Width = Size.Grow();
    public Size Height = Size.Grow();
    public Spacing Margin = Spacing.Zero();
    public Spacing Padding = Spacing.Zero();
    public Color BackgroundColor = new Color(0,0,0,0);
    public Texture2D? BackgroundTexture;
    public NPatchInfo? NPatch;

    public float BorderRadius = 0;
    public float BorderWidth = 0;
    public Color BorderColor = Color.Black;

    public string Text = "";
    public float TextSize = 14f;
    public float TextSpacing = 4f;
    public Color TextColor = Color.Gray;
    public Font TextFont = RUI.DefaultFont;
    public Vector2 TextMeasurement = Vector2.Zero;


    // |-------------Outer-------------|
    // |            Margin             |
    // |  |---------Boarder---------|  |
    // |  | |--------Inner--------| |  |
    // |  | |       Padding       | |  |
    // |  | |   |---Content---|   | |  |
    // |  | |   |             |   | |  |
    public Rectangle OuterRect;
    public Rectangle InnerRect;
    public Rectangle ContentRect;
    
    public UIElement? Parent { get; internal set; }
    private List<UIElement> Children { get; }  = [];
    
    private void SetPosition(Vector2 position)
    {
        var marginOffset = new Vector2(Margin.Left, Margin.Top);
        var paddingOffset = new Vector2(Padding.Left, Padding.Top);
        OuterRect.Position = position;
        InnerRect.Position = position + marginOffset;
        ContentRect.Position = position + marginOffset + paddingOffset;
    }
    
    private void SetWidth(float value)
    {
        OuterRect.Width = value;
        InnerRect.Width = value - Margin.Width;
        ContentRect.Width = value - Margin.Width - Padding.Width;
    }

    private void SetHeight(float value)
    {
        OuterRect.Height = value;
        InnerRect.Height = value - Margin.Height;
        ContentRect.Height = value - Margin.Height - Padding.Height;
    }

    public UIElement SetParent(UIElement? parent)
    {
        Parent?.Children.Remove(this);
        Parent = parent;
        Parent?.Children.Add(this);
        return this;
    }

    public UIElement Add(params UIElement[] children)
    {
        foreach (var child in children)
        {
            child.Parent = this;
            Children.Add(child);
        }

        return this;
    }

    public void Clear()
    {
        Children.Clear();
    }

    public void SetSize(Vector2 size)
    {
        SetWidth(size.X);
        SetHeight(size.Y);
    }

    public void LayoutFitWidth(Vector2 space)
    {
        if (Width.Type == Size.Mode.Value)
        {
            SetWidth(Width.Value);
            space.X = ContentRect.Width;
            foreach (var child in Children) child.LayoutFitWidth(space);
        } else {
            var minWidth = Layout == LayoutMode.Horizontal ? (Children.Count - 1) * Gap : 0;
            foreach (var child in Children)
            {
                child.LayoutFitWidth(space);
                switch (Layout)
                {
                    case LayoutMode.Horizontal:
                        // For horizontal layout, sum up all child widths
                        minWidth += child.OuterRect.Width;
                        break;  
                    case LayoutMode.Vertical:
                        // For vertical layout, find the maximum child width
                        minWidth = Math.Max(minWidth, child.OuterRect.Width);
                        break;
                }
            }
            
            if (Width.Type == Size.Mode.Fit)
                SetWidth(minWidth + Margin.Width + Padding.Width);
        }
    }

    public void LayoutGrowShrinkWidth(Vector2 space)
    {
        if (Children.Count == 0) return;
        
        // First, calculate grow/shrink for all children recursively
        var totalChildrenWidth = 0f;
        var totalChildrenFactor = 0f;
        foreach (var child in Children)
        {
            totalChildrenWidth += child.OuterRect.Width;
            if (child.Width.Type == Size.Mode.Grow)
            {
                totalChildrenFactor += child.Width.Value;
            }
        }
        
        // Calculate remaining space to distribute - this might be negative so then we shrink
        var remainingSpace = ContentRect.Width - ((Children.Count - 1) * Gap) - totalChildrenWidth;
        foreach (var child in Children)
        {
            if (child.Width.Type == Size.Mode.Grow)
            {
                switch (Layout)
                {
                    case LayoutMode.Horizontal:
                        var extraWidth = remainingSpace * (child.Width.Value / totalChildrenFactor);
                        child.SetWidth(Math.Max(0, child.OuterRect.Width + extraWidth));
                        break;
                    case LayoutMode.Vertical:
                        child.SetWidth(ContentRect.Width * child.Width.Value);
                        break;
                }
            }
            child.LayoutGrowShrinkWidth(ContentRect.Size);
        }
    }

    public void LayoutWrapText(Vector2 space)
    {
        // TODO: Implement
        TextMeasurement = MeasureTextEx(TextFont, Text, TextSize, TextSpacing);
        if (ContentRect.Width < TextMeasurement.X)
        {
            SetWidth(float.Min(TextMeasurement.X + Margin.Width + Padding.Width, space.X));
        }
        space = ContentRect.Size;
        foreach (var element in Children) element.LayoutWrapText(space);
    }

    public void LayoutFitHeight(Vector2 space)
    {
        if (Height.Type == Size.Mode.Value)
        {
            SetHeight(Height.Value);
            space.Y = ContentRect.Height;
            foreach (var child in Children) child.LayoutFitHeight(space);
        } else {
            var minHeight = Layout == LayoutMode.Vertical ? (Children.Count - 1) * Gap : 0;
            foreach (var child in Children)
            {
                child.LayoutFitHeight(space);
                switch (Layout)
                {
                    case LayoutMode.Horizontal:
                        // For horizontal layout, find the maximum child height
                        minHeight = Math.Max(minHeight, child.OuterRect.Height);
                        break;
                    case LayoutMode.Vertical:
                        // For vertical layout, sum up all child heights
                        minHeight += child.OuterRect.Height;
                        break;
                }
            }
            
            if (Height.Type == Size.Mode.Fit)
                SetHeight(minHeight + Margin.Height + Padding.Height);
        }
    }

    public void LayoutGrowShrinkHeight(Vector2 space)
    {
        if (Children.Count == 0) return;
        
        // First, calculate grow/shrink for all children recursively
        var totalHeight = 0f;
        var totalGrowFactor = 0f;
        foreach (var child in Children)
        {
            totalHeight += child.OuterRect.Height;
            if (child.Height.Type == Size.Mode.Grow)
            {
                totalGrowFactor += child.Height.Value;
            }
        }
        
        // Calculate remaining space to distribute - this might be negative so then we shrink
        var remainingSpace = ContentRect.Height - ((Children.Count - 1) * Gap) - totalHeight;
        foreach (var child in Children)
        {
            if (child.Height.Type == Size.Mode.Grow)
            {
                switch (Layout)
                {
                    case LayoutMode.Horizontal:
                        child.SetHeight(ContentRect.Height * child.Height.Value);
                        break;
                    case LayoutMode.Vertical:
                        var extraHeight = remainingSpace * (child.Height.Value / totalGrowFactor);
                        child.SetHeight(Math.Max(0, child.OuterRect.Height + extraHeight));
                        break;
                }
            }
            child.LayoutGrowShrinkHeight(ContentRect.Size);
        }
    }

    public void CalculatePosition(Vector2 position)
    {
        SetPosition(position);

        if (Children.Count == 0)
            return;

        var totalMain = (Children.Count - 1) * Gap;
        foreach (var child in Children)
        {
            switch (Layout)
            {
                case LayoutMode.Horizontal:
                    totalMain += child.OuterRect.Width;
                    break;
                case LayoutMode.Vertical:
                    totalMain += child.OuterRect.Height;
                    break;
            }
        }
        totalMain = Math.Max(0, totalMain);
        
        var cursor = Layout switch
        {
            LayoutMode.Horizontal => ChildAlignment switch
            {
                Alignment.TopLeft or Alignment.CenterLeft or Alignment.BottomLeft
                    => new Vector2(ContentRect.X, 0),
                Alignment.TopCenter or Alignment.CenterCenter or Alignment.BottomCenter
                    => new Vector2(ContentRect.X + (ContentRect.Width - totalMain) / 2, 0),
                Alignment.TopRight or Alignment.CenterRight or Alignment.BottomRight
                    => new Vector2(ContentRect.X + ContentRect.Width - totalMain, 0),
                _ => new Vector2(ContentRect.X, 0)
            },
            LayoutMode.Vertical => ChildAlignment switch
            {
                Alignment.TopLeft or Alignment.TopCenter or Alignment.TopRight
                    => new Vector2(0, ContentRect.Y),
                Alignment.CenterLeft or Alignment.CenterCenter or Alignment.CenterRight
                    => new Vector2(0, ContentRect.Y + (ContentRect.Height - totalMain) / 2),
                Alignment.BottomLeft or Alignment.BottomCenter or Alignment.BottomRight
                    => new Vector2(0, ContentRect.Y + ContentRect.Height - totalMain),
                _ => new Vector2(0, ContentRect.Y)
            },
            _ => Vector2.Zero
        };

        foreach (var child in Children)
        {
            var final = cursor + child.Offset;
            switch (Layout)
            {
                case LayoutMode.Horizontal:
                    final.Y = ChildAlignment switch
                    {
                        Alignment.TopLeft or Alignment.TopCenter or Alignment.TopRight
                            => ContentRect.Y + child.OffsetY,
                        Alignment.CenterLeft or Alignment.CenterCenter or Alignment.CenterRight
                            => ContentRect.Y + (ContentRect.Height - child.OuterRect.Height) / 2 + child.OffsetY,
                        Alignment.BottomLeft or Alignment.BottomCenter or Alignment.BottomRight
                            => ContentRect.Y + ContentRect.Height - child.OuterRect.Height + child.OffsetY,
                        _ => final.Y
                    };
                    break;
                case LayoutMode.Vertical:
                    final.X = ChildAlignment switch
                    {
                        Alignment.TopLeft or Alignment.CenterLeft or Alignment.BottomLeft
                            => ContentRect.X + child.OffsetX,
                        Alignment.TopCenter or Alignment.CenterCenter or Alignment.BottomCenter
                            => ContentRect.X + (ContentRect.Width - child.OuterRect.Width) / 2 + child.OffsetX,
                        Alignment.TopRight or Alignment.CenterRight or Alignment.BottomRight
                            => ContentRect.X + ContentRect.Width - child.OuterRect.Width + child.OffsetX,
                        _ => final.X
                    };
                    break;
            }

            child.CalculatePosition(final);

            if (Layout == LayoutMode.Horizontal)
                cursor.X += child.OuterRect.Width + Gap;
            else
                cursor.Y += child.OuterRect.Height + Gap;
        }
    }

    public virtual void Update(float dt)
    {
        foreach (var child in Children) child.Update(dt);
    }

    public void Render()
    {
        RUI.Logger.LogInformation($"{Name} rect {InnerRect}");
        
        DrawBackground();
        foreach (var element in Children) element.Render();
        DrawText();
        DrawBoarder();
    }
    
    private void DrawBackground()
    {
        if (BackgroundColor.A <= 0) return;

        if (BackgroundTexture.HasValue)
        {
            var tex = BackgroundTexture.Value;
            if (NPatch.HasValue)
            {
                DrawTextureNPatch(tex, NPatch.Value, InnerRect, Vector2.Zero, 0, BackgroundColor);
            } else {
                var srcRect = new Rectangle(0, 0, tex.Width, tex.Height);
                DrawTexturePro(tex, srcRect, InnerRect, Vector2.Zero, 0, BackgroundColor);
            }
        }
        else
        {
            if (BorderRadius > 0)
            {
                DrawRectangleRounded(InnerRect, BorderRadius, 12, BackgroundColor);
            }
            else
            {
                DrawRectangleRec(InnerRect, BackgroundColor);
            }
        }
    }

    private void DrawText()
    {
        if (Text == "") return;
        DrawTextPro(TextFont, Text, ContentRect.Position + (ContentRect.Size * 0.5f), TextMeasurement * 0.5f, 0, TextSize, TextSpacing, TextColor);
    }

    private void DrawBoarder()
    {
        if (BorderWidth > 0)
        {
            if (BorderRadius > 0)
            {
                DrawRectangleRoundedLinesEx(InnerRect, BorderRadius, 8, BorderWidth, BorderColor);
            }
            else
            {
                DrawRectangleLinesEx(InnerRect, BorderWidth, BorderColor);
            }
        }
    }
}
