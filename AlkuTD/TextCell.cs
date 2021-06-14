using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
	public delegate void TextInputDelegate();

    public class TextCell : Button
    {
        /*public enum DropDownMenuPos
        {
            Below,
            Above,
            Left,
            Right
        }
        public DropDownMenuPos DDMenuPos;*/

        public int Index;
        public bool Overwrite;
		public event TextInputDelegate InputMade;

        public TextCell(string text, int xPos, int yPos, int width, int height, int padding, TextAlignment textAlign, Color[] buttonColors, Color[] textColors, Texture2D texture, InputType inputType, bool dropDownMenu, bool overwrite)
            : base (text,xPos,yPos,width,height,padding,textAlign,buttonColors,textColors,texture)
        {
            this.inputType = inputType;
			Overwrite = overwrite;
            IsDropDownMenu = dropDownMenu;

        }

        /*public void PopulateDropDownMenu(string[] items)
        {
            DropDownButtons = new Button[items.Length];
            //if (Pos.Y < Game1.pixel.GraphicsDevice.Viewport.Height/2)  //----------------------------------------muuten kiva mutta muu taulukko tulee deferredissä päälle---------------------------
            if (DDMenuPos == DropDownMenuPos.Below)
                for (int i = 0; i < items.Length; i++)
                    DropDownButtons[i] = new Button(items[i], Pos.X, Pos.Y + (i+1)*Height, Width, Height, Padding, TextAlign, ButtonColors, TextColors, ButtonTexture);
            else if (DDMenuPos == DropDownMenuPos.Right)
                for (int i = 0; i < items.Length; i++)
                    DropDownButtons[i] = new Button(items[i], Pos.X + Width, Pos.Y + i*Height, Width, Height, Padding, TextAlign, ButtonColors, TextColors, ButtonTexture);
           // else
           //     for (int i = 0; i < items.Length; i++)
            //        DropDownButtons[i] = new Button(items[i], Pos.X, Pos.Y - (i+1)*Height, Width, Height, Padding, TextAlign, ButtonColors, TextColors, ButtonTexture);

        }*/

		void OnInputMade()
		{
			if (InputMade != null)
				InputMade();
		}

		void TrimZeroes()
		{
			while (Text.Length > 1 && Text.StartsWith("0"))
				Text = Text.Remove(0, 1);
		}

		 /// <summary>
        /// Tries to convert keyboard input to characters and prevents repeatedly returning the 
        /// same character if a key was pressed last frame, but not yet unpressed this frame.
        /// </summary>
        /// <param name="keyboard">The current KeyboardState</param>
        /// <param name="oldKeyboard">The KeyboardState of the previous frame</param>
        /// <param name="c">When this method returns, contains the correct character if conversion succeeded.
        /// Else contains the null, (000), character.</param>
        /// <returns>True if conversion was successful</returns>
        public static bool KeysToChar(Keys key, out char c)
        {
			bool shift = CurrentGame.keyboard.IsKeyDown(Keys.LeftShift) || CurrentGame.keyboard.IsKeyDown(Keys.RightShift);            
        
            switch (key)
            {
                //Alphabet keys
                case Keys.A: if (shift) { c = 'A'; } else { c = 'a'; } return true;
                case Keys.B: if (shift) { c = 'B'; } else { c = 'b'; } return true;
                case Keys.C: if (shift) { c = 'C'; } else { c = 'c'; } return true;
                case Keys.D: if (shift) { c = 'D'; } else { c = 'd'; } return true;
                case Keys.E: if (shift) { c = 'E'; } else { c = 'e'; } return true;
                case Keys.F: if (shift) { c = 'F'; } else { c = 'f'; } return true;
                case Keys.G: if (shift) { c = 'G'; } else { c = 'g'; } return true;
                case Keys.H: if (shift) { c = 'H'; } else { c = 'h'; } return true;
                case Keys.I: if (shift) { c = 'I'; } else { c = 'i'; } return true;
                case Keys.J: if (shift) { c = 'J'; } else { c = 'j'; } return true;
                case Keys.K: if (shift) { c = 'K'; } else { c = 'k'; } return true;
                case Keys.L: if (shift) { c = 'L'; } else { c = 'l'; } return true;
                case Keys.M: if (shift) { c = 'M'; } else { c = 'm'; } return true;
                case Keys.N: if (shift) { c = 'N'; } else { c = 'n'; } return true;
                case Keys.O: if (shift) { c = 'O'; } else { c = 'o'; } return true;
                case Keys.P: if (shift) { c = 'P'; } else { c = 'p'; } return true;
                case Keys.Q: if (shift) { c = 'Q'; } else { c = 'q'; } return true;
                case Keys.R: if (shift) { c = 'R'; } else { c = 'r'; } return true;
                case Keys.S: if (shift) { c = 'S'; } else { c = 's'; } return true;
                case Keys.T: if (shift) { c = 'T'; } else { c = 't'; } return true;
                case Keys.U: if (shift) { c = 'U'; } else { c = 'u'; } return true;
                case Keys.V: if (shift) { c = 'V'; } else { c = 'v'; } return true;
                case Keys.W: if (shift) { c = 'W'; } else { c = 'w'; } return true;
                case Keys.X: if (shift) { c = 'X'; } else { c = 'x'; } return true;
                case Keys.Y: if (shift) { c = 'Y'; } else { c = 'y'; } return true;
                case Keys.Z: if (shift) { c = 'Z'; } else { c = 'z'; } return true;

                //Decimal keys
                case Keys.D0: if (shift) { c = ')'; } else { c = '0'; } return true;
                case Keys.D1: if (shift) { c = '!'; } else { c = '1'; } return true;
                case Keys.D2: if (shift) { c = '@'; } else { c = '2'; } return true;
                case Keys.D3: if (shift) { c = '#'; } else { c = '3'; } return true;
                case Keys.D4: if (shift) { c = '$'; } else { c = '4'; } return true;
                case Keys.D5: if (shift) { c = '%'; } else { c = '5'; } return true;
                case Keys.D6: if (shift) { c = '^'; } else { c = '6'; } return true;
                case Keys.D7: if (shift) { c = '&'; } else { c = '7'; } return true;
                case Keys.D8: if (shift) { c = '*'; } else { c = '8'; } return true;
                case Keys.D9: if (shift) { c = '('; } else { c = '9'; } return true;

                //Decimal numpad keys
                case Keys.NumPad0: c = '0'; return true;
                case Keys.NumPad1: c = '1'; return true;
                case Keys.NumPad2: c = '2'; return true;
                case Keys.NumPad3: c = '3'; return true;
                case Keys.NumPad4: c = '4'; return true;
                case Keys.NumPad5: c = '5'; return true;
                case Keys.NumPad6: c = '6'; return true;
                case Keys.NumPad7: c = '7'; return true;
                case Keys.NumPad8: c = '8'; return true;
                case Keys.NumPad9: c = '9'; return true;
                    
                //Special keys
                case Keys.OemTilde: if (shift) { c = '~'; } else { c = '`'; } return true;
                case Keys.OemSemicolon: if (shift) { c = ':'; } else { c = ';'; } return true;
                case Keys.OemQuotes: if (shift) { c = '"'; } else { c = '\''; } return true;
                case Keys.OemQuestion: if (shift) { c = '?'; } else { c = '/'; } return true;
                case Keys.OemPlus: if (shift) { c = '+'; } else { c = '='; } return true;
                case Keys.OemPipe: if (shift) { c = '|'; } else { c = '\\'; } return true;
                case Keys.OemPeriod: if (shift) { c = '>'; } else { c = '.'; } return true;
                case Keys.OemOpenBrackets: if (shift) { c = '{'; } else { c = '['; } return true;
                case Keys.OemCloseBrackets: if (shift) { c = '}'; } else { c = ']'; } return true;
                case Keys.OemMinus: if (shift) { c = '_'; } else { c = '-'; } return true;
                case Keys.OemComma: if (shift) { c = '<'; } else { c = ','; } return true;
                case Keys.Space: c = ' '; return true;                                       
            }

            c = (char)0;
            return false;           
        }

        int backspaceRefreshCounter; //-------------------------not elegant....?
        public bool justActivated;

		string prevText = "";
        public void Update(MouseState mouse, MouseState prevMouse, KeyboardState keyboard, KeyboardState prevKeyboard)
        {
            /*if (justActivated)
            {
                State = State == ButnState.Active ? ButnState.Passive : ButnState.Active;

            }*/
            if (State == ButnState.Released)
            {
                if ((prevKeyboard.IsKeyDown(Keys.LeftShift) || prevKeyboard.IsKeyDown(Keys.LeftControl)) && prevKeyboard.IsKeyUp(Keys.LeftAlt))
                {
                    if (PrevState == ButnState.Active || PrevState == ButnState.Passive)
                        State = ButnState.Selected;
                    else State = ButnState.Passive;
                }
                else
                    State = PrevState == ButnState.Active || PrevState == ButnState.Selected ? ButnState.Passive : ButnState.Active;

            }

            if (State == ButnState.Active)
            {
                if (IsDropDownMenu && DropDownButtons != null)
                {
                    if ((keyboard.IsKeyDown(Keys.Down) && prevKeyboard.IsKeyUp(Keys.Down)) && SelectedItem < DropDownButtons.Length -1)
                        SelectedItem++;
                    else if ((keyboard.IsKeyDown(Keys.Up) && prevKeyboard.IsKeyUp(Keys.Up)) && SelectedItem > 0)
                        SelectedItem--;

                    for (int i = 0; i < DropDownButtons.Length; i++)
                    {
                        DropDownButtons[i].Update(mouse, prevMouse);
                        
                        if (DropDownButtons[i].State == ButnState.Released)
                        {
                            Text = DropDownButtons[i].Text;
                            SelectedItem = (byte)i;
                            //if (Text == "" || Text == "-" || Text == "red" || Text == "green" || Text == "blue" || Text == "brown" || Text == "purple" || Text == "turq" || Text == "all")
                                ButtonColors = DropDownButtons[i].ButtonColors;
                        }
                    }
                    //if ()
                    if (keyboard.IsKeyDown(Keys.Enter))
                    {
                        Text = DropDownButtons[SelectedItem].Text;
                        ButtonColors = DropDownButtons[SelectedItem].ButtonColors;
                        State = ButnState.Selected;
                    }
                    else /*if (SelectedItem >= 0)*/
                        DropDownButtons[SelectedItem].State = ButnState.Active;
                }
                else // not dropdown = NORMAL IN ACTIVE STATE
                {
                    #region TEXT INPUT
                    if (keyboard.IsKeyUp(Keys.Back)) backspaceRefreshCounter = 20;
                    if (keyboard.IsKeyUp(Keys.LeftControl))
                    {
                        foreach (Keys key in keyboard.GetPressedKeys())
                        {
                            //byte ascii = (byte)key;
                            if (!(key == Keys.Space || key == Keys.Enter || key == Keys.Back || /*key == Keys.Left || key == Keys.Up || key == Keys.Right || key == Keys.Down ||*/ key == Keys.OemPeriod || key == Keys.OemComma || key == Keys.Decimal) && !((byte)key >= 48 && (byte)key <= 90) && !((byte)key >= 96 && (byte)key <= 105))
                                continue; //unpractical exclusion of keys

                            switch (inputType) // exclude wrong types of input
                            {
                                case InputType.text: break; // exclude nothing
                                case InputType.integer: if (((byte)key >= 65 && (byte)key <= 90) || key == Keys.OemPeriod || key == Keys.Decimal || key == Keys.OemComma || key == Keys.Space) continue; break; // exclude letters, period/comma/decimal and space
                                case InputType.floating: if ((byte)key >= 65 && (byte)key <= 90 || key == Keys.Space) continue; break; // exclude letters and space
                            }

                            if (key == Keys.Back)
                            {
                                if (backspaceRefreshCounter == 20 && Text.Length > 0)
                                    Text = Text.Remove(Text.Length - 1, 1);
                                else if (backspaceRefreshCounter == 0 && Text.Length > 0)
                                {
                                    Text = Text.Remove(Text.Length - 1, 1);
                                    backspaceRefreshCounter = 4;
                                }
                                backspaceRefreshCounter--;
                            }
                            else if (prevKeyboard.IsKeyUp(key))
                            {
                                if (key == Keys.Enter)
                                {
                                    State = ButnState.Passive;
									TrimZeroes();
									OnInputMade();
                                }
                                else
                                {
                                    if (Overwrite && justActivated)
                                    {
                                        Text = "";
                                    }
                                    if (key == Keys.Space) Text += " ";
									else if ((byte)key >= 48 && (byte)key <= 57) // ord nums
									{
										if (key != Keys.D0 && Text.StartsWith("0") && !Text.Contains('.'))
											Text = Text.Remove(0, 1);
										if (!(key == Keys.D0 && Text.Length == 1 && Text.StartsWith("0")))
											Text += (int)key - 48;
									}
									else if ((byte)key >= 96 && (byte)key <= 105) //numpad nums
									{
										if (key != Keys.NumPad0 && Text.StartsWith("0") && !Text.Contains('.'))
											Text = Text.Remove(0, 1);
										if (!(key == Keys.NumPad0 && Text.Length == 1 && Text.StartsWith("0")))
											Text += (int)key - 96;
									}
									else if (key == Keys.OemPeriod || ((key == Keys.OemComma || key == Keys.Decimal) && inputType == InputType.floating)) Text += '.';
									else if (key == Keys.OemComma || key == Keys.Decimal) Text += ',';
									else if (!keyboard.IsKeyDown(Keys.LeftShift) && !keyboard.IsKeyDown(Keys.RightShift)) Text += key.ToString().ToLower();
									else Text += key.ToString();
                                }
                                justActivated = false;
                            }
                        }
                    }
                    #endregion
                }
            }
            
            if (Bounds.Contains(mouse.X, mouse.Y))
            {
                if (mouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed)
                {
                    //if (Index == 18)
                    //    if (false) ;
                    //State = State == ButnState.Active ? ButnState.Passive : ButnState.Active;
                    justActivated = true;

                    if (Text.Length > 1 && inputType == InputType.integer && Text.StartsWith("0"))
                        Text = Text.Remove(0, 1);
                    
                    State = ButnState.Released;
                }
                else if (mouse.LeftButton == ButtonState.Pressed)
                {
                    if (State != ButnState.Active && State != ButnState.Selected)
                        State = ButnState.Pressed;
                }
                else if (State != ButnState.Active && State != ButnState.Selected)
                    State = ButnState.Hovered;

                //MouseOverSomeButton = true;
            }
            else if (State != ButnState.Passive &&
                     (State != ButnState.Active && State != ButnState.Selected ||
                     (mouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed && !(keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.LeftShift)))))
            {
                State = ButnState.Passive;

				if (inputType == InputType.integer) //Text.ElementAt<char>(0) == '0')
					TrimZeroes();

				if (Text != prevText)
					OnInputMade();
                //MouseOverSomeButton = false;
                //if (Index == 3)
                //    if (false) ;
            }

            if (PrevState != State /*&& State != ButnState.Hovered*/ && State != ButnState.Pressed && State != ButnState.Released)
                PrevState = State;

			if (State == ButnState.Passive)
				prevText = Text;
        }
    }
}
