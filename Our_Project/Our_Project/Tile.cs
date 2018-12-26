﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our_Project
{
    class Tile
    {
        private Texture2D texture;              //the isometric tile texture
        private Texture2D cartasian_texture;    //the 2D tile for debugging.
        public int tilesize;
        public Rectangle Rec;

        Rectangle isoprojection_rectangle; //the rectangle in which we draw the isometric projection

        public Tile left;
        public Tile right;
        public Tile down;
        public Tile up;
        public Occupied occupied;

        public enum Occupied
        {
            no, yes_by_me, yes_by_enemy
        }

        public Tile(Texture2D _texture, Texture2D _cartasian_texture, Rectangle rec)
        {
            tilesize = rec.Width;
            texture = _texture;
            cartasian_texture = _cartasian_texture;
            Rec = rec;
            occupied = Occupied.no;
        }

        
        protected void Update()
        {

        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {

            //dubug draw of the cartasian 2d-real world tiles.
           // spriteBatch.Draw(cartasian_texture, Rec, null, color, MathHelper.ToRadians(0f), new Vector2(0), SpriteEffects.None, 0f);

            //creating the isometric-screen rectangle where we will draw our tile.
            Vector2 iso_location = Game1.TwoD2isometrix(Rec.X, Rec.Y);
            isoprojection_rectangle = new Rectangle((int)iso_location.X-tilesize, (int)iso_location.Y, Rec.Width*2, Rec.Height);

            spriteBatch.Draw(texture, isoprojection_rectangle, null, color,MathHelper.ToRadians(0f),new Vector2(0),SpriteEffects.None,0f);



        }


    }
}