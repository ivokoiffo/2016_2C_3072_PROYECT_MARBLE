using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Collision;
using TGC.Core.Geometry;

namespace TGC.Group.Model.Utils
{
    class CollitionManager
    {

        public static List<BoundingBoxCollider> obstaculos { get; set; }

        public static Boolean detectColision(BoundingBoxCollider boundingBox)
        {
            Boolean collide = false;
            foreach (BoundingBoxCollider obstaculo in CollitionManager.obstaculos)
            {

                
                TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(boundingBox.Aabb, obstaculo.Aabb);
                if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                {
                    collide = true;
                    break;
                }
            }
            return collide;
        }
        public static List<BoundingBoxCollider> getColisions(BoundingBoxCollider boundingBox)
        {
            List<BoundingBoxCollider> boundingBoxes = new List<BoundingBoxCollider>();
            foreach (BoundingBoxCollider obstaculo in CollitionManager.obstaculos)
            {

                TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(boundingBox.Aabb, obstaculo.Aabb);
                if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                {
                    boundingBoxes.Add(obstaculo);
                }
            }
            return boundingBoxes;
        }

        public static List<BoundingBoxCollider> getColisions(TgcRay ray)
        {
            Vector3 vector = new Vector3();
            return CollitionManager.obstaculos.FindAll(b => TgcCollisionUtils.intersectRayAABB(ray, b.Aabb, out vector));
        }

        public static Boolean isColliding(BoundingBoxCollider boundingBox, BoundingBoxCollider obstaculo)
        {
            TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(boundingBox.Aabb, obstaculo.Aabb);
            return result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando;
        }

        public static Boolean getClosestBoundingBox(TgcRay rayCast, out BoundingBoxCollider boundingBoxResult, BoundingBoxCollider boundingBox)
        {
            List<BoundingBoxCollider> boundingBoxes = getColisions(rayCast);
            boundingBoxes.Remove(boundingBox);
            if (boundingBoxes.Count == 0)
            {
                boundingBoxResult = null;
                return false;
            }
            else
            {
                List<Vector3> vectors = boundingBoxes.ConvertAll(b => { Vector3 vector = new Vector3(); TgcCollisionUtils.intersectRayAABB(rayCast, b.Aabb, out vector); return vector; });
                boundingBoxResult = boundingBoxes.Find(b => { Vector3 vector = new Vector3(); TgcCollisionUtils.intersectRayAABB(rayCast, b.Aabb, out vector); return vectors.TrueForAll(v => Vector3.Length(vector - rayCast.Origin) <= Vector3.Length(v - rayCast.Origin)); });
                return true;
            }
        }

        public static Vector3 getClosesPointBetween(TgcRay rayCast, BoundingBoxCollider boundingBox)
        {
            Vector3 vector = new Vector3();
            TgcCollisionUtils.intersectRayAABB(rayCast, boundingBox.Aabb, out vector);
            return vector;
        }


    }
}

