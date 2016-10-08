using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Utils;
using TGC.Core.Camara;

namespace TGC.Group.Model
{
	
   public  class CamaraPrimeraPersona : TgcCamera
    {
		private  Point mouseCenter { get; set;} //Centro de mause 2D para ocultarlo.

		//Se mantiene la matriz rotacion para no hacer este calculo cada vez.
		private Matrix cameraRotation;

		//Direction view se calcula a partir de donde se quiere ver con la camara inicialmente. por defecto se ve en -Z.
		private Vector3 directionView;

		//No hace falta la base ya que siempre es la misma, la base se arma segun las rotaciones de esto costados y updown.
		private float leftrightRot;
		private float updownRot;

		private Vector3 positionEye;

        public float MovementSpeed { get; set; }

		public float RotationSpeed { get; set; }

		private TgcD3dInput Input { get; set; }


		public CamaraPrimeraPersona(TgcD3dInput input,Vector3 position)
		{
			Input = input;
			positionEye = new Vector3();
			mouseCenter = new Point(
				D3DDevice.Instance.Device.Viewport.Width / 2,
				D3DDevice.Instance.Device.Viewport.Height / 2);
			RotationSpeed = 0.1f;
			MovementSpeed = 500f;
			directionView = new Vector3(0, 0, -1);
			leftrightRot = FastMath.PI_HALF;
			updownRot = -FastMath.PI / 10.0f;
			cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);
		}
		public  void UpdateCamara(float elapsedTime) {

			var moveVector = new Vector3(0, 1, 0);
				leftrightRot -= -Input.XposRelative * RotationSpeed;
			updownRot -= Input.YposRelative * RotationSpeed;
			//Se actualiza matrix de rotacion, para no hacer este calculo cada vez y solo cuando en verdad es necesario.
			cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);
			//Calculamos la nueva posicion del ojo segun la rotacion actual de la camara.
			var cameraRotatedPositionEye = Vector3.TransformNormal(moveVector * elapsedTime, cameraRotation);
			positionEye += cameraRotatedPositionEye;

			//Calculamos el target de la camara, segun su direccion inicial y las rotaciones en screen space x,y.
			var cameraRotatedTarget = Vector3.TransformNormal(directionView, cameraRotation);
			var cameraFinalTarget = positionEye + cameraRotatedTarget;
			var cameraOriginalUpVector = DEFAULT_UP_VECTOR;
			var cameraRotatedUpVector = Vector3.TransformNormal(cameraOriginalUpVector, cameraRotation);

			base.SetCamera(positionEye, cameraFinalTarget, cameraRotatedUpVector);

		}
		public override void SetCamera(Vector3 position, Vector3 directionView)
		{
			positionEye = position;
			this.directionView = directionView;
		}
	}
}
