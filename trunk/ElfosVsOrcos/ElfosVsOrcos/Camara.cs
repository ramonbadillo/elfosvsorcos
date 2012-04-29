using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace ElfosVsOrcos
{
    public class Camara
    {
        protected float          _zoom; // Camera Zoom
        public Matrix             _transform; // Matrix Transform
        public Vector2          _pos; // Camera Position
        protected float         _rotation; // Camera Rotation
 
        public Camara()
        {
            _zoom = 1.0f;
            _rotation = 0.0f;
            _pos = Vector2.Zero;
        }


        // Sets and gets zoom
        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = value; if (_zoom < 0.1f) _zoom = 0.1f; } // Negative zoom will flip image
        }
 
        public float Rotation
        {
            get {return _rotation; }
            set { _rotation = value; }
        }
 
        // Auxiliary function to move the camera
        public void Move(Vector2 amount)
        {
           _pos += amount;
        }


        public void MoveLeft(float amount)
        {
            this._pos += new Vector2((float)(Math.Cos(-this._rotation + MathHelper.Pi) * amount), (float)(Math.Sin(-this._rotation + MathHelper.Pi) * amount));
            
        }

        public void MoveRight(float amount)
        {
            this._pos += new Vector2((float)(Math.Cos(-this._rotation) * amount), (float)(Math.Sin(-this._rotation) * amount));
            
        }

        public void ZoomCam(float amount) {
            _zoom += amount;
        }

       // Get set position
        public Vector2 Pos
        {
             get{ return  _pos; }
             set{ _pos = value; }
        }


        public Matrix get_transformation(GraphicsDevice graphicsDevice)
        {
            _transform =       // Thanks to o KB o for this solution
              Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0)) *
                                         Matrix.CreateRotationZ(Rotation) *
                                         Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                                         Matrix.CreateTranslation(new Vector3(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f, 0));
            return _transform;
        }

    }
}
