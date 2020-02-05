using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace AlkuTD
{
	public enum ButnState : byte
	{
		Passive,
		Hovered,
		Pressed,
		Released,
		Selected,
		Active
	}
    public enum InputType { text, integer, floating }
    public enum DropDownMenuPos { Below, Above, Left, Right }
    public enum TextAlignment { Center, Left, Right }

    public class Button
    {
        public static bool MouseOverSomeButton;
        public bool SuppressUpdate;

        public InputType inputType;
        public DropDownMenuPos DDMenuPos;
        TextAlignment textAlign;
        public TextAlignment TextAlign { get { return textAlign; } set { textAlign = value; RealignText(); } }

        public bool IsDropDownMenu;
        public Button[] DropDownButtons;
        public byte SelectedItem;

        public bool InToggleMode;
        public ButnState State;
        public ButnState PrevState;
        string text;
        public string Text { get { return text; } set { text = value; RealignText(); } }
        //public int TextSize;
        public Vector2 TextDimensions;
        public int Padding;
        public int YPadding;
        public Vector2 TextPos;
        

        public Rectangle Bounds;
          public Point Pos { get { return new Point(Bounds.X, Bounds.Y); } set { Bounds.Location = new Point((int)value.X, (int)value.Y); RealignText(); } }
          public int Width { get { return Bounds.Width; } set { Bounds.Width = value; } }
          public int Height { get { return Bounds.Height; } set { Bounds.Height = value; } }

        public Color[] ButtonColors;
        public Color[] TextColors;
        public Texture2D ButtonTexture;

        public bool NonDeselectGroup;

        public Button(string text, int xPos, int yPos, TextAlignment textAlign, Color[] buttonColors, Color[] textColors, Texture2D texture)
        {
            Text = text;

            TextDimensions.X = CurrentGame.font.MeasureString(text).X;
            TextDimensions.Y = CurrentGame.font.MeasureString(text == "" ? "A" : text.ToUpper()).Y;
            Padding = 10;
            Bounds = new Rectangle(xPos, yPos, (int)Math.Round(TextDimensions.X) + 2*Padding, (int)Math.Round(TextDimensions.Y) + Padding);

            ButtonColors = buttonColors;
            TextColors = textColors;
            ButtonTexture = texture;
            TextAlign = textAlign;

            //State = ButState.Passive;
            RealignText();
        }
        public Button(string text, int xPos, int yPos, int width, int height, int padding, TextAlignment textAlign, Color[] buttonColors, Color[] textColors, Texture2D texture)
            : this(text,xPos,yPos,textAlign,buttonColors,textColors,texture)
        {
            Padding = padding;
            Bounds = new Rectangle(xPos, yPos, width, height);
            RealignText();
        }
        public Button(string text, int xPos, int yPos, int width, int height, int padding, TextAlignment textAlign, Color[] buttonColors, Color[] textColors, Texture2D texture, InputType inputType, bool isDropDownMenu)
            : this(text,xPos,yPos,textAlign,buttonColors,textColors,texture)
        {
            Padding = padding;
            Bounds = new Rectangle(xPos, yPos, width, height);
            this.inputType = inputType;
            IsDropDownMenu = isDropDownMenu;
            RealignText();
        }
        public Button(string text, int xPos, int yPos, int width, int height, int padding, TextAlignment textAlign, Color buttonColor, Color textColor, Texture2D texture)
            : this(text,xPos,yPos,width,height,padding,textAlign, new Color[]{buttonColor,buttonColor,buttonColor}, new Color[]{textColor,textColor,textColor}, texture)
        { 
            RealignText();
        }

        public void PopulateDropDownMenu(string[] items)
        {
            DropDownButtons = new Button[items.Length];

            int largestWordWidth = 0;
            for (int i = 0; i < items.Length; i++)
                if (CurrentGame.font.MeasureString(items[i]).X > largestWordWidth)
                    largestWordWidth = (int)CurrentGame.font.MeasureString(items[i]).X + Padding*2;

            //if (Pos.Y < Game1.pixel.GraphicsDevice.Viewport.Height/2)  //----------------------------------------muuten kiva mutta muu taulukko tulee deferredissä päälle---------------------------
            if (DDMenuPos == DropDownMenuPos.Below)
                for (int i = 0; i < items.Length; i++)
                    DropDownButtons[i] = new Button(items[i], Pos.X, Pos.Y + (i+1)*Height, Width, Height, Padding, TextAlign, ButtonColors, TextColors, ButtonTexture);
            else if (DDMenuPos == DropDownMenuPos.Right)
                for (int i = 0; i < items.Length; i++)
                    DropDownButtons[i] = new Button(items[i], Pos.X + Width, Pos.Y + i*Height, largestWordWidth, Height, Padding, TextAlign, ButtonColors, TextColors, ButtonTexture);
            // else
            //     for (int i = 0; i < items.Length; i++)
            //        DropDownButtons[i] = new Button(items[i], Pos.X, Pos.Y - (i+1)*Height, Width, Height, Padding, TextAlign, ButtonColors, TextColors, ButtonTexture);

        }

        public void RealignText()
        {
            TextDimensions.X = CurrentGame.font.MeasureString(text).X;
            if (TextAlign == TextAlignment.Left)
                TextPos = new Vector2(Bounds.X + Padding, Bounds.Center.Y - (int)TextDimensions.Y/2 + YPadding);
            else if (TextAlign == TextAlignment.Center)
                TextPos = new Vector2(Bounds.Center.X - (int)TextDimensions.X/2, Bounds.Center.Y - (int)TextDimensions.Y/2 + YPadding);
            else //(TextAlign == TextAlignment.Right)
                TextPos = new Vector2(Bounds.Right - (int)TextDimensions.X - Padding, Bounds.Center.Y - (int)TextDimensions.Y/2 + YPadding);
        }

        public void ResizeBounds()
        {
            Bounds.Width = (int)Math.Round(TextDimensions.X) + 2 * Padding;
        }

        /*public int WaitForDecision()
        {
            
        }*/

        byte prevItem;
        public void Update(MouseState mouse, MouseState prevMouse)
        {
            if (State == ButnState.Active && PrevState != ButnState.Active)
                PrevState = ButnState.Active;
            else if (State != ButnState.Active && PrevState != ButnState.Passive)
                PrevState = ButnState.Passive;

            if (IsDropDownMenu && DropDownButtons != null)
            {
                if (State == ButnState.Active || State == ButnState.Released)
                {
                    /*if ((CurrentGame.keyboard.IsKeyDown(Keys.Down) && CurrentGame.prevKeyboard.IsKeyUp(Keys.Down)) && SelectedItem < DropDownButtons.Length -1)
                        SelectedItem++;
                    else if ((CurrentGame.keyboard.IsKeyDown(Keys.Up) && CurrentGame.prevKeyboard.IsKeyUp(Keys.Up)) && SelectedItem > 0)
                        SelectedItem--;*/

                    for (int i = 0; i < DropDownButtons.Length; i++)
                    {
                        DropDownButtons[i].Update(mouse, prevMouse);

                        if (DropDownButtons[i].State == ButnState.Released)
                        {
                            SelectedItem = (byte)i;
                            //DropDownButtons[i].State = ButnState.Passive;
                        }
                    }
                }
                prevItem = SelectedItem;
            }

            if (Bounds.Contains(mouse.X, mouse.Y))
            {
                if (mouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed)
                {
                    if (InToggleMode || IsDropDownMenu)
                        State = State == ButnState.Active ? ButnState.Hovered : ButnState.Active;
                    else
                        State = ButnState.Released;
                }
                else if (State != ButnState.Active && mouse.LeftButton == ButtonState.Pressed)
                    State = ButnState.Pressed;
                else if (State != ButnState.Active)
                    State = ButnState.Hovered;

                //if (NonDeselectGroup)
                //    MouseOverSomeButton = true;
            }
            else if (State != ButnState.Passive &&
                     (!(InToggleMode || IsDropDownMenu) ||
                      (mouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed && State == ButnState.Active) || 
                      (State != ButnState.Active && (InToggleMode || IsDropDownMenu))))
            {
                /*if (NonDeselectGroup && !MouseOverSomeButton)
                {
                    MouseOverSomeButton = false;
                    State = ButnState.Passive;
                }
                else if (!NonDeselectGroup)*/
                State = ButnState.Passive;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(ButtonTexture, Bounds, ButtonColors[Math.Min((int)State, 2)]);
            sb.DrawString(CurrentGame.font, Text, TextPos, TextColors[Math.Min((int)State, 2)]);
            if (IsDropDownMenu && DropDownButtons != null && State == ButnState.Active)
            {
                for (int i = 0; i < DropDownButtons.Length; i++)
                    DropDownButtons[i].Draw(sb);
            }
        }

        public override string ToString()
        {
            return ("\"" + Text + "\"").PadRight(11, '_') + " " + State.ToString(); //en dash –
        }
    }
}
