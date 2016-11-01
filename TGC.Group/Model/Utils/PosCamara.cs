using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    public class PosCamara
    {
        public Vector3 lookAt;
        public Vector3 posicion;

        public PosCamara(Vector3 posicion, Vector3 lookAt)
        {
            this.posicion = posicion;
            this.lookAt = lookAt;
        }
    }
}
