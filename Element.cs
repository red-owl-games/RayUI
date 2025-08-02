using System.Numerics;
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
    public Size Width = Size.Grow();
    public Size Height = Size.Grow();
    public Spacing Margin = Spacing.Zero();
    public Spacing Padding = Spacing.Zero();
    public Color BackgroundColor = new Color(0,0,0,0);

    public float BorderRadius = 0;
    public float BorderWidth = 0;
    public Color BorderColor = Color.Black;

    public Rectangle OuterRect;
    public Rectangle InnerRect;
    public Rectangle ContentRect;
    // |-------------Outer-------------|
    // |            Margin             |
    // |  |---------Boarder---------|  |
    // |  | |--------Inner--------| |  |
    // |  | |       Padding       | |  |
    // |  | |   |---Content---|   | |  |
    // |  | |   |             |   | |  |
    
    public UIElement? Parent { get; internal set; }
    private List<UIElement> Children { get; }  = [];

    public void SetParent(UIElement parent)
    {
        Parent?.Children.Remove(this);
        Parent = parent;
        Parent.Children.Add(this);
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

    public void LayoutFitWidth(Vector2 space)
    {
        if (Width.Type == Size.Mode.Value)
        {
            OuterRect.Width = Width.Value;
            InnerRect.Width = Width.Value - Margin.Width;
            ContentRect.Width = Width.Value - Margin.Width - Padding.Width;
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
            
            OuterRect.Width = minWidth + Margin.Width + Padding.Width;
            InnerRect.Width = minWidth + Padding.Width;
            ContentRect.Width = minWidth;
        }
    }

    public void LayoutGrowShrinkWidth(Vector2 space)
    {
        if (Children.Count == 0) return;
        
        // First, calculate grow/shrink for all children recursively
        var totalWidth = 0f;
        var totalGrowFactor = 0f;
        foreach (var child in Children)
        {
            child.LayoutGrowShrinkWidth(space);
            totalWidth += child.OuterRect.Width;
            if (child.Width.Type == Size.Mode.Grow)
            {
                totalGrowFactor += child.Width.Value;
            }
        }
        
        // Calculate remaining space to distribute - this might be negative so then we shrink
        float remainingSpace = ContentRect.Width - totalWidth;
        foreach (var child in Children)
        {
            if (child.Width.Type == Size.Mode.Grow)
            {
                // Calculate proportional space for this child
                float extraWidth = (remainingSpace * child.Width.Value) / totalGrowFactor;
                child.OuterRect.Width = Math.Max(0, child.OuterRect.Width + extraWidth);
                child.InnerRect.Width = Math.Max(0, child.InnerRect.Width + extraWidth);
                child.ContentRect.Width = Math.Max(0, child.ContentRect.Width + extraWidth);
            }
        }
    }

    public void LayoutWrapText(Vector2 space)
    {
        // TODO: Implement
        space = ContentRect.Size;
        foreach (var element in Children) element.LayoutWrapText(space);
    }

    public void LayoutFitHeight(Vector2 space)
    {
        if (Height.Type == Size.Mode.Value)
        {
            OuterRect.Height = Height.Value;
            InnerRect.Height = Height.Value - Margin.Height;
            ContentRect.Height = Height.Value - Margin.Height - Padding.Height;
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
            
            OuterRect.Height = minHeight + Margin.Height + Padding.Height;
            InnerRect.Height = minHeight + Padding.Height;
            ContentRect.Height = minHeight;
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
            child.LayoutGrowShrinkHeight(space);
            totalHeight += child.OuterRect.Height;
            if (child.Height.Type == Size.Mode.Grow)
            {
                totalGrowFactor += child.Height.Value;
            }
        }
        
        // Calculate remaining space to distribute - this might be negative so then we shrink
        float remainingSpace = ContentRect.Height - totalHeight;
        foreach (var child in Children)
        {
            if (child.Height.Type == Size.Mode.Grow)
            {
                // Calculate proportional space for this child
                float extraHeight = (remainingSpace * child.Height.Value) / totalGrowFactor;
                child.OuterRect.Height = Math.Max(0, child.OuterRect.Height + extraHeight);
                child.InnerRect.Height = Math.Max(0, child.InnerRect.Height + extraHeight);
                child.ContentRect.Height = Math.Max(0, child.ContentRect.Height + extraHeight);
            }
        }
    }

    public void CalculatePosition(Vector2 position)
    {
        // Set the outer rectangle position
        OuterRect.X = position.X;
        OuterRect.Y = position.Y;
        
        // Calculate inner rectangle position (outer + margin)
        InnerRect.X = OuterRect.X + Margin.Left;
        InnerRect.Y = OuterRect.Y + Margin.Top;
        
        // Calculate content rectangle position (inner + padding)
        ContentRect.X = InnerRect.X + Padding.Left;
        ContentRect.Y = InnerRect.Y + Padding.Top;
        
        // Early return if no children
        if (Children.Count == 0) return;
        float totalWidth = Layout == LayoutMode.Horizontal ? (Children.Count - 1) * Gap : 0;
        float totalHeight = Layout == LayoutMode.Vertical ? (Children.Count - 1) * Gap : 0;
        foreach (var child in Children)
        {
            switch (Layout)
            {
                case LayoutMode.Horizontal:
                    totalWidth += child.OuterRect.Width;
                    totalHeight = Math.Max(totalHeight, child.OuterRect.Height);
                    break;
                case LayoutMode.Vertical:
                    totalWidth = Math.Max(totalWidth, child.OuterRect.Width);
                    totalHeight += child.OuterRect.Height;
                    break;
            }
        }
        
        totalWidth = Math.Max(0, totalWidth);
        totalHeight = Math.Max(0, totalHeight);
        Vector2 childPosition = ChildAlignment switch
        {
            Alignment.TopLeft => new Vector2(ContentRect.X, ContentRect.Y),
            Alignment.TopCenter => new Vector2(ContentRect.X + (ContentRect.Width - totalWidth) / 2, ContentRect.Y),
            Alignment.TopRight => new Vector2(ContentRect.X + ContentRect.Width - totalWidth, ContentRect.Y),
            Alignment.CenterLeft => new Vector2(ContentRect.X, ContentRect.Y + (ContentRect.Height - totalHeight) / 2),
            Alignment.CenterCenter => new Vector2(ContentRect.X + (ContentRect.Width - totalWidth) / 2, ContentRect.Y + (ContentRect.Height - totalHeight) / 2),
            Alignment.CenterRight => new Vector2(ContentRect.X + ContentRect.Width - totalWidth, ContentRect.Y + (ContentRect.Height - totalHeight) / 2),
            Alignment.BottomLeft => new Vector2(ContentRect.X, ContentRect.Y + ContentRect.Height - totalHeight),
            Alignment.BottomCenter => new Vector2(ContentRect.X + (ContentRect.Width - totalWidth) / 2, ContentRect.Y + ContentRect.Height - totalHeight),
            Alignment.BottomRight => new Vector2(ContentRect.X + ContentRect.Width - totalWidth, ContentRect.Y + ContentRect.Height - totalHeight),
            _ => new Vector2(ContentRect.X, ContentRect.Y) // Default to TopLeft
        };
        
        // Position children with their individual offsets
        foreach (var child in Children)
        {
            // Apply OffsetX/OffsetY to the calculated position
            Vector2 finalPosition = childPosition + new Vector2(
                child.OffsetX.Type == Size.Mode.Value ? child.OffsetX.Value : 0,
                child.OffsetY.Type == Size.Mode.Value ? child.OffsetY.Value : 0
            );
            
            child.CalculatePosition(finalPosition);
            
            // Update position for next child based on layout direction
            switch (Layout)
            {
                case LayoutMode.Horizontal:
                    childPosition.X += child.OuterRect.Width + Gap;
                    break;
                case LayoutMode.Vertical:
                    childPosition.Y += child.OuterRect.Height + Gap;
                    break;
            }
        }
    }

    public virtual void Update(float dt) {}

    public void Render()
    {
        if (BackgroundColor.A > 0)
        {
            if (BorderRadius > 0)
            {
                DrawRectangleRounded(InnerRect, BorderRadius, 8, BackgroundColor);
            }
            else
            {
                DrawRectangleRec(InnerRect, BackgroundColor);
            }
        }

        foreach (var element in Children) element.Render();

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
