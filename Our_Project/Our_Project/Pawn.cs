﻿
/* Gil Nevo 310021654
 * Shachar Bartal 305262016
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XELibrary;

namespace Our_Project
{
   public class Pawn
    {
        public int id;

        public Tile current_tile;  // the tile that now the pawn is in.
        private Tile direction;          // if we will move, this will get the details of the new tile.

        public bool the_flag;
        
        public int strength;
        public bool send_update;
        public bool isMouseClicked; // if we clicked on pawn 
        private bool hasMoved;           // if the pawn has moved

        public bool attacked=false;
        public Pawn attacker;
        private bool hasDied = false;

        public MouseState oldState; // mouse input old position.
        public Vector2 position;

        private readonly SpriteFont strength_font;

        public String flag_animation;

        private double timer_atk_num_display=0; // timer for displaying attack.

        private bool draw_atk_font = false;

        public static readonly float scaleOfFont = Game1.screen_height * 0.00055f; // for scaling our fonts to different screens.
        private double timer_has_died = 0;

        Rectangle mouseRec;
        public Team team;
        public enum Team
        {
            my_team, enemy_team
        }
        ICelAnimationManager celAnimationManager;

        public Pawn(Game game,String _flag_animation, Tile _tile, int _strength, Team _team, int _id, SpriteFont _strength_font)
        {
            celAnimationManager = (ICelAnimationManager)game.Services.GetService(typeof(ICelAnimationManager));

            flag_animation = _flag_animation; //setting animation.


            if (_strength == 21)  //setting the king of flags.
                the_flag = true;

            id = _id; // pawn id (always equals strength).

            current_tile = _tile; // the current tile we are on.

            _tile.SetCurrentPawn(this); // telling the tile we are on it.

            team = _team;

            strength_font = _strength_font;
            
           _tile.occupied = Tile.Occupied.yes_by_me;
          
            strength = _strength;
           
           
            isMouseClicked = false;
            hasMoved = false;

            position = new Vector2(_tile.GetCartasianRectangle().X, _tile.GetCartasianRectangle().Y);

            send_update = true; //initialize as true it tells the server to update this pawn for the second player.
        }

        public void Update(GameTime gametime)
        {
           
            MouseState mouseState = Mouse.GetState(); // previous mouse position
            MouseState newState = Mouse.GetState();     // current mouse position  
            
            //the location of the world mouse.
            Vector2 CartasianMouseLocation = Game1.Isometrix2twoD(mouseState.X, mouseState.Y);

            //rectangle of the world mouse.
             mouseRec = new Rectangle((int)CartasianMouseLocation.X, (int)CartasianMouseLocation.Y, 1, 1);

            //rectangle of the screen mouse.
            // mouseRec = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

            // clicking on pawn.
            if ((newState.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released) &&
                        (mouseRec.Intersects(current_tile.GetCartasianRectangle())))
            {
                if (!isMouseClicked) // if we want to move
                    isMouseClicked = true;
                else                 // if we want to cancel moving
                    isMouseClicked = false;
            }

            oldState = newState; // set the mouse old position to be the current position.

            if (attacked)
            {
                GettingAttacked(gametime);
            }

            if (isMouseClicked && !hasDied && !the_flag) //if the pawn didnt die and he is not the king of flags and was clicked on.
            {
                // getting the newe details of the  mouse position
                newState = Mouse.GetState();
                CartasianMouseLocation = Game1.Isometrix2twoD(mouseState.X, mouseState.Y);
                mouseRec.X =(int) CartasianMouseLocation.X;
                mouseRec.Y =(int) CartasianMouseLocation.Y;

                // if there is another click, that means we want to move to a new direction.
                if (newState.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Pressed)
                {
                    // checking if the direction is legal:
                    if ((current_tile.GetLeft() != null) && (!current_tile.GetLeft().GetIsHidden()) &&
                        (current_tile.GetLeft().occupied != Tile.Occupied.yes_by_me) &&  (mouseRec.Intersects(current_tile.GetLeft().GetCartasianRectangle())))

                    {
                        MoveORattack(current_tile.GetLeft(), gametime);
                    }


                    else if ((current_tile.GetRight() != null) && (!current_tile.GetRight().GetIsHidden()) &&
                        (current_tile.GetRight().occupied != Tile.Occupied.yes_by_me) && (mouseRec.Intersects(current_tile.GetRight().GetCartasianRectangle())))

                    {
                        MoveORattack(current_tile.GetRight(), gametime);
                    }

                    else if ((current_tile.GetUp() != null) && (!current_tile.GetUp().GetIsHidden()) &&
                        (current_tile.GetUp().occupied != Tile.Occupied.yes_by_me) && (mouseRec.Intersects(current_tile.GetUp().GetCartasianRectangle())))

                    {
                        MoveORattack(current_tile.GetUp() , gametime);
                    }

                    else if ((current_tile.GetDown() != null) && (!current_tile.GetDown().GetIsHidden()) &&
                        (current_tile.GetDown().occupied != Tile.Occupied.yes_by_me) && (mouseRec.Intersects(current_tile.GetDown().GetCartasianRectangle())))

                    {
                        MoveORattack(current_tile.GetDown() , gametime);
                    }
                    if (hasMoved && !hasDied) 
                    {
                        oldState = newState;

                       
                        {
                            // moving the pawn and assigning new values to related tiles.
                            current_tile.occupied = Tile.Occupied.no;
                            current_tile.SetCurrentPawn(null);
                            current_tile = direction;
                            current_tile.occupied = Tile.Occupied.yes_by_me;
                            current_tile.SetCurrentPawn(this);
                            send_update = true;
                        }
                    }
                    else if (hasDied)
                        {

                            current_tile.occupied = Tile.Occupied.no;
                            current_tile.SetCurrentPawn(null);
                            isMouseClicked = false;

                        }
                }

            }
            else if (hasDied) //if the pawn is dead we cannot move or click on him.
            {

                current_tile.occupied = Tile.Occupied.no;
                current_tile.SetCurrentPawn(null);
                isMouseClicked = false;
                
            }
        }

        //if we got information from the server about an attack.
        public void GettingAttacked(GameTime gametime)
        {
            timer_atk_num_display += gametime.ElapsedGameTime.TotalSeconds; //timer to display attack number on screen.
            attacker.timer_atk_num_display+= gametime.ElapsedGameTime.TotalSeconds; ;
            draw_atk_font = true;
            attacker.draw_atk_font = true;

            if (the_flag) // if enemy found the flag we lost.
            {
                PlayingState.lose = true;
            }

            //if we lost the encounter with enemy
            if (attacker.strength > strength)
            {
                timer_has_died = gametime.ElapsedGameTime.TotalSeconds;
               // hasDied = true;

            }
            //if we won the encounter with the enemy
            else if (attacker.strength < strength)
            {
                attacker.timer_has_died= gametime.ElapsedGameTime.TotalSeconds;
              //  attacker.hasDied = true;

            }
            //if we draw in the encounter with the enemy.
            else if (attacker.strength == strength)
            {
                timer_has_died = gametime.ElapsedGameTime.TotalSeconds;
                attacker.timer_has_died = gametime.ElapsedGameTime.TotalSeconds;
              //  hasDied = true;
              //  attacker.hasDied = true;

            }
            attacked = false; //resetting value
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            //timers handling
            if(draw_atk_font)
            timer_atk_num_display += gameTime.ElapsedGameTime.TotalSeconds;

            if (timer_has_died > 0 && !hasDied)
                timer_has_died += gameTime.ElapsedGameTime.TotalSeconds;

            if (timer_has_died > 3.0)
                hasDied = true;

            //if pawn is still alive.
            if (!hasDied)
            {

                if (team == Team.my_team)
                {
                    Rectangle Rec = new Rectangle(Game1.TwoD2isometrix(current_tile.GetCartasianRectangle().Center) - new Point(Tile.GetTileSize() / 2), new Point(Tile.GetTileSize()));
                    celAnimationManager.Draw(gameTime, flag_animation, spriteBatch, Rec, SpriteEffects.None);

                    //if king of flags.
                    if (the_flag)
                        spriteBatch.DrawString(strength_font, "flag", Game1.TwoD2isometrix(current_tile.GetCartasianRectangle().Center.X, current_tile.GetCartasianRectangle().Center.Y), Color.White, 0, Vector2.Zero, scaleOfFont, SpriteEffects.None, 0);
                    //if regular pawn
                    else
                    spriteBatch.DrawString(strength_font, strength.ToString(), Game1.TwoD2isometrix(current_tile.GetCartasianRectangle().Center.X, current_tile.GetCartasianRectangle().Center.Y), Color.White, 0, Vector2.Zero, scaleOfFont, SpriteEffects.None, 0);

                    //timer for drawing attack.
                    if (timer_atk_num_display<3.0 && draw_atk_font)

                        spriteBatch.DrawString(strength_font, strength.ToString(), Game1.TwoD2isometrix((int)position.X,(int)position.Y)+new Vector2(-Game1.screen_width/10,-Game1.screen_height/10 - 20 * (float)timer_atk_num_display), Color.Green, 0, Vector2.Zero, scaleOfFont*10, SpriteEffects.None, 0);
                    else //resetting values.
                    {
                        draw_atk_font = false;
                        timer_atk_num_display = 0;
                    }
                }
                else //pawn of the enemy's team
                {
                    Rectangle Rec = new Rectangle(Game1.TwoD2isometrix(current_tile.GetCartasianRectangle().Center) - new Point(Tile.GetTileSize() / 2), new Point(Tile.GetTileSize()));
                    celAnimationManager.Draw(gameTime, flag_animation, spriteBatch, Rec, SpriteEffects.None);

                    if (timer_atk_num_display < 3.0 && draw_atk_font)
                        spriteBatch.DrawString(strength_font, strength.ToString(), Game1.TwoD2isometrix((int)position.X, (int)position.Y) + new Vector2(+Game1.screen_width / 10, -Game1.screen_height / 10 - 20 * (float)timer_atk_num_display), Color.Red, 0, Vector2.Zero, scaleOfFont, SpriteEffects.None, 0);
                    else
                    {
                        draw_atk_font = false;
                        timer_atk_num_display = 0;
                    }
                }
            }
            //if pawn is dead we draw it in the dead section.
            else
            {

                if (team == Team.my_team)
                {
                    //   spriteBatch.Draw(texture, new Rectangle(30 * strength, 20, Tile.getTileSize() / 2, Tile.getTileSize() / 2), Color.White);
                    celAnimationManager.Draw(gameTime, flag_animation, spriteBatch, new Rectangle(Game1.screen_width / 40 * strength, Game1.screen_height/10, Tile.GetTileSize() / 2, Tile.GetTileSize() / 2), SpriteEffects.None);
                    spriteBatch.DrawString(strength_font, strength.ToString(), new Vector2(Game1.screen_width / 40 * strength, Game1.screen_height / 10), Color.White, 0, Vector2.Zero, scaleOfFont, SpriteEffects.None, 0);
                    
                }
                else
                {
                    //  spriteBatch.Draw(texture, new Rectangle(320 + 30 * strength, 20, Tile.getTileSize() / 2, Tile.getTileSize() / 2), Color.White);
                    celAnimationManager.Draw(gameTime, flag_animation, spriteBatch, new Rectangle( Game1.screen_width / 40 * strength, Game1.screen_height / 10 * 8, Tile.GetTileSize() / 2, Tile.GetTileSize() / 2), SpriteEffects.None);
                    spriteBatch.DrawString(strength_font, strength.ToString(), new Vector2(Game1.screen_width / 40 * strength, Game1.screen_height / 10 * 8), Color.White, 0, Vector2.Zero, scaleOfFont, SpriteEffects.None, 0);
                }
                

            }

            //drawing adjecant tiles if clicked
            if (isMouseClicked)
            {
                if ((current_tile.GetLeft() != null) && (current_tile.GetLeft().occupied == Tile.Occupied.no))
                    current_tile.GetLeft().SetColor(Color.Red);


                if ((current_tile.GetRight() != null) && (current_tile.GetRight().occupied == Tile.Occupied.no))
                    current_tile.GetRight().SetColor(Color.Red);

                if ((current_tile.GetUp() != null) && (current_tile.GetUp().occupied == Tile.Occupied.no))
                    current_tile.GetUp().SetColor(Color.Red);

                if ((current_tile.GetDown() != null) && (current_tile.GetDown().occupied == Tile.Occupied.no))
                    current_tile.GetDown().SetColor(Color.Red);

            }

            if (hasMoved) //drawing adjecant to white after pawn moved.
            { 


                if ((current_tile.GetLeft() != null) && (current_tile.GetLeft().occupied == Tile.Occupied.no))
                    current_tile.GetLeft().SetColor(Color.White);

                if ((current_tile.GetRight() != null) && (current_tile.GetRight().occupied == Tile.Occupied.no))
                    current_tile.GetRight().SetColor(Color.White);

                if ((current_tile.GetUp() != null) && (current_tile.GetUp().occupied == Tile.Occupied.no))
                    current_tile.GetUp().SetColor(Color.White);

                if ((current_tile.GetDown() != null) && (current_tile.GetDown().occupied == Tile.Occupied.no))
                    current_tile.GetDown().SetColor(Color.White);

                isMouseClicked = false;
                hasMoved = false;
                direction = null;
            }

            //drawing the world mouse for debug purposes.
            //spriteBatch.Draw(texture,new Rectangle(mouseRec.Location,new Point(10)) , Color.Goldenrod);
  
        }

        //if pawned move to an empty tile or enemy tile or teleport tile.
       private void MoveORattack(Tile _direction, GameTime gametime)
        {
            hasMoved = true;

            direction = _direction; //tile we moved to

            

            if (direction.occupied == Tile.Occupied.yes_by_enemy) // starting encounter with enemy
            {
                direction.GetCurrentPawn().attacked = true;
                direction.GetCurrentPawn().attacker = this;

                //setting timers for display
                timer_atk_num_display += gametime.ElapsedGameTime.TotalSeconds; //timer to display attack number on screen.
                direction.GetCurrentPawn().timer_atk_num_display += gametime.ElapsedGameTime.TotalSeconds; ;
               
                draw_atk_font = true;
                direction.GetCurrentPawn().draw_atk_font = true;

                if (direction.GetCurrentPawn().the_flag == true) //if we found the enemy's king of flags,
                {
                    PlayingState.win = true;
                }
                    //if we lost the encounter with enemy
                else if (direction.GetCurrentPawn().strength > strength)

                {
                    timer_has_died = gametime.ElapsedGameTime.TotalSeconds;
                    //hasDied = true;
                }
                else //if we draw the encounter with enemy
                if (direction.GetCurrentPawn().strength == strength)
                {
                    timer_has_died = gametime.ElapsedGameTime.TotalSeconds;
                   // hasDied = true;
                   // direction.getCurrentPawn().hasDied = true;
                    direction.GetCurrentPawn().timer_has_died = gametime.ElapsedGameTime.TotalSeconds;
                }
                else //if we won the encounter with enemy
                {
                    // direction.getCurrentPawn().hasDied = true;
                    direction.GetCurrentPawn().timer_has_died = gametime.ElapsedGameTime.TotalSeconds;
                }

            }

            //checking to see if encounterd a teleport.
            if (direction.teleport_tile && timer_has_died == 0)
            {
                direction = direction.Teleport_to_rand();
                
            }
            send_update = true;
        }


    }
}