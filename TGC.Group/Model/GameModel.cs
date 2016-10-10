using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Utils;
using System;
using TGC.Core.BoundingVolumes;
using System.Collections.Generic;
using TGC.Core.SkeletalAnimation;
using TGC.Group.Model.Utils;
using System.Windows.Forms;
using TGC.Core.Collision;
namespace TGC.Group.Model
{
    public class GameModel : TgcExample
	{
        private TgcScene escenario;
        private TgcSkeletalMesh personaje;
        private TgcBoundingElipsoid boundPersonaje;
        private TgcBoundingElipsoid boundMonstruo;
        private TgcSkeletalMesh monstruo;
        private TgcMesh unMesh;
        private bool flagGod = false;
		private Matrix cameraRotation;
		private float leftrightRot;
		private float updownRot;
		public float RotationSpeed { get; set; }
		private Vector3 viewVector;
		Vector3 lookAt;

        double rot = 0;
        private bool jumping;
        double variacion;
        private float jumpingElapsedTime;
        private readonly List<Collider> objetosColisionables = new List<Collider>();
		private readonly List<Collider> armarios = new List<Collider>();
		private readonly List<TgcMesh> puertas = new List<TgcMesh>();
        private ElipsoidCollisionManager collisionManager;
        float larg = 4;

        private Checkpoint ClosestCheckPoint;
        List<TgcArrow> ArrowsClosesCheckPoint;
		private TgcMesh setMeshToOrigin(TgcMesh mesh)
 		{
 			//Desplazar los vertices del mesh para que tengan el centro del AABB en el origen
 			var center = mesh.BoundingBox.calculateBoxCenter();
 			mesh=moveMeshVertices(-center, mesh);
 
 			//Ubicar el mesh en donde estaba originalmente
 			mesh.BoundingBox.setExtremes(mesh.BoundingBox.PMin - center, mesh.BoundingBox.PMax - center);
 			mesh.Position = center;
 			return mesh;
 		}
 
 		private TgcMesh moveMeshVertices(Vector3 offset, TgcMesh mesh)
 		{	
 			switch (mesh.RenderType)
 			{
 			case TgcMesh.MeshRenderType.VERTEX_COLOR:
 				var verts1 = (TgcSceneLoader.VertexColorVertex[])mesh.D3dMesh.LockVertexBuffer(typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
 				for (var i = 0; i<verts1.Length; i++)
 				{
 					verts1[i].Position = verts1[i].Position + offset;
 				}
 				mesh.D3dMesh.SetVertexBufferData(verts1, LockFlags.None);
 				mesh.D3dMesh.UnlockVertexBuffer();
 					return mesh;
 			
 
 			case TgcMesh.MeshRenderType.DIFFUSE_MAP:
 				var verts2 = (TgcSceneLoader.DiffuseMapVertex[])mesh.D3dMesh.LockVertexBuffer(typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
 				for (var i = 0; i<verts2.Length; i++)
 				{
 					verts2[i].Position = verts2[i].Position + offset;
 				}
 				mesh.D3dMesh.SetVertexBufferData(verts2, LockFlags.None);
 				mesh.D3dMesh.UnlockVertexBuffer();
 				return mesh;
 
 			case TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
 				var verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])mesh.D3dMesh.LockVertexBuffer(typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
 				for (var i = 0; i<verts3.Length; i++)
 				{
 					verts3[i].Position = verts3[i].Position + offset;
 				}
 				mesh.D3dMesh.SetVertexBufferData(verts3, LockFlags.None);
 				mesh.D3dMesh.UnlockVertexBuffer();
 				return mesh;
 			}
 			return mesh;
 		}


       
		public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

		//Caja que se muestra en el ejemplo.
        private TgcBox Box { get; set; }

        //Mesh de TgcLogo.
        private TgcMesh Mesh { get; set; }

        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }

        private void seteoDePersonaje() {
            //Cargar personaje con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            personaje =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\BasicHuman\\BasicHuman-TgcSkeletalMesh.xml",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Walk-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\StandBy-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Jump-TgcSkeletalAnim.xml"
                    });
            //IMPORTANTE PREGUNTAR PORQUE DEBERIA ESTAR DESHABILITADO AUTOTRANSFORM
            personaje.AutoTransformEnable = true;
            //Escalarlo porque es muy grande
            personaje.Position = new Vector3(82f,110f, 886);
            //Escalamos el personaje ya que sino la escalera es demasiado grande.
            personaje.Scale = new Vector3(1.0f, 1.0f, 1.0f);
            boundPersonaje = new TgcBoundingElipsoid(personaje.BoundingBox.calculateBoxCenter() + new Vector3(0, 0, 0), new Vector3(12, 28, 12));
            jumping = false;
        }
        private void seteoDelMonstruo()
        {
            //Paths para archivo XML de la malla
            var pathMesh = MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml";

            //Path para carpeta de texturas de la malla
            var mediaPath = MediaDir + "SkeletalAnimations\\Robot\\";

            //Lista de animaciones disponibles
            string[] animationList =
            {
                "Parado",
                "Caminando",
                "Correr",
                "PasoDerecho",
                "PasoIzquierdo",
                "Empujar",
                "Patear",
                "Pegar",
                "Arrojar"
            };

            //Crear rutas con cada animacion
            var animationsPath = new string[animationList.Length];
            for (var i = 0; i < animationList.Length; i++)
            {
                animationsPath[i] = mediaPath + animationList[i] + "-TgcSkeletalAnim.xml";
            }

            //Cargar mesh y animaciones
            var loader = new TgcSkeletalLoader();
            monstruo = loader.loadMeshAndAnimationsFromFile(pathMesh, mediaPath, animationsPath);

            monstruo.AutoTransformEnable = true;
            //Escalarlo porque es muy grande
            monstruo.Position = new Vector3(325,101, 475);
            //Escalamos el personaje ya que sino la escalera es demasiado grande.
            monstruo.Scale = new Vector3(0.65f, 0.65f, 0.65f);

            monstruo.playAnimation(animationList[0], true);

            //boundMonstruo = new TgcBoundingElipsoid(personaje.BoundingBox.calculateBoxCenter() + new Vector3(0, 0, 0), new Vector3(12, 28, 12));
        
            
        }
        public override void Init()
        {
            //Para la creacion de checkpoints, borrar en el futuro
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            Clipboard.Clear();
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;
			d3dDevice.ShowCursor(true);
            //Seteo el personaje
            seteoDePersonaje();
            //Seteo del monsturo
            seteoDelMonstruo();
            //Seteo el escenario
            escenario = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Mapa\\MPmapa+El1ConArmario-TgcScene.xml");

            leftrightRot = FastMath.PI_HALF;
			updownRot = -FastMath.PI / 10.0f;
			cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);
			RotationSpeed = 0.1f;
			viewVector = new Vector3(-150,0,100);
			//initPuertaGiratoria();   
			//Almacenar volumenes de colision del escenario
			objetosColisionables.Clear();
            CollisionManager.obstaculos = new List<BoundingBoxCollider>();
            foreach (var mesh in escenario.Meshes)
            {
				if (mesh.Name.Contains("Puerta"))
				{
					mesh.AutoTransformEnable = true;
					puertas.Add(mesh);
				
				}
				if (mesh.Name.Contains("Placard")|| mesh.Name.Contains("Locker")) {
					armarios.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
				}
                objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
                CollisionManager.obstaculos.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
            }

            CheckpointHelper.BuildCheckpoints();
           

            //Crear manejador de colisiones
            collisionManager = new ElipsoidCollisionManager();
            collisionManager.GravityEnabled = true;


        }
 
        private void godMod() {
            Camara = new CamaraGod(personaje.Position,Input);
        }

        private void animacionDePuerta(TgcMesh unMesh) {
       
            if (Input.keyPressed(Key.U))
            {
				
				if (rot >= 1.57)
				{
					rot = 1.57;
					variacion = -0.9 * ElapsedTime;
				};
				if (rot <= 0)
				{
					rot = 0;
					variacion = 0.9 * ElapsedTime;
				};
				rot += variacion;
				var ang = System.Convert.ToSingle(rot);

				unMesh.rotateY(ang);
               unMesh.move(new Vector3(System.Convert.ToSingle((larg - (Math.Cos(rot + 3.14) * larg))), 0, System.Convert.ToSingle(Math.Sin(rot + 3.14) * larg)));
			
				//Si superamos cierto Y volvemos a la posición original.
				//if (Camara.Position.Y > 300f)
				// {
				//     Camara.SetCamera(new Vector3(Camara.Position.X, 0f, Camara.Position.Z), Camara.LookAt);
				//  }
			}
           
           }
        private void setCamaraPrimeraPersona(Vector3 lookAt) {
            Vector3 posicionConOffset = Vector3.Add(new Vector3(5,20,2),(boundPersonaje.Center));
            Camara.SetCamera(posicionConOffset,lookAt);
        }
	
			   //seteo de velocidades
			private void controlDeArmario(Collider mesh)
		{
			if ((boundPersonaje.Center - mesh.BoundingSphere.Center).Length() < (boundPersonaje.Radius.Length() + mesh.BoundingSphere.Radius))
			{
				
				Camara.SetCamera(Vector3.Add(mesh.BoundingSphere.Center,new Vector3(1,1,1)), lookAt*(-1));
			}
		}

		public void rotarPuerta(TgcMesh puerta) {
			Vector3 pos = puerta.Position;
			puerta.Position = new Vector3(0, 0, 0);
			puerta.Transform = Matrix.RotationZ(1);
			puerta.Position = pos;
		}
		public void moverPersonaje(){    
		var velocidadCaminar = 1.0f;
			var velocidadRotacion =25;
            var velocidadSalto = 1.0f;
            var tiempoSalto = 1.0f;

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;
            float jump = 0;
			var marchaAtras = false;
            //Adelante
            if (Input.keyDown(Key.W)){
                moveForward = -velocidadCaminar;
                moving = true;
            }


            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = velocidadCaminar;
                moving = true;
        		marchaAtras = true;

			}else if (moving){
                //Activar animacion de caminando
                personaje.playAnimation("Walk", true);
            }else{
                personaje.playAnimation("StandBy", true);
            }


            //Vector de movimiento
            var movementVector = Vector3.Empty;
			var leftrightRotPrevius= leftrightRot-Input.XposRelative * RotationSpeed;
			var updownRotPrevius = updownRot + Input.YposRelative * RotationSpeed;
		    leftrightRot -= Input.XposRelative * RotationSpeed; 
			personaje.rotateY(Input.XposRelative* RotationSpeed);

			var movem=new Vector3(0,0,0);
			if (moving)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                movem = new Vector3(FastMath.Sin(moveForward)*velocidadCaminar,0,FastMath.Cos(moveForward)*velocidadCaminar);
				//Se actualiza matrix de rotacion, para no hacer este calculo cada vez y solo cuando en verdad es necesario.
				//if(!marchaAtras) viewVector = movementVector; //Solo cambia el vector de view si no esta caminando para atras
			}
			//maximos para los giros del vectorDeView
			if (-1f < updownRotPrevius && updownRotPrevius < 1f) { updownRot += Input.YposRelative * RotationSpeed; }

				cameraRotation = Matrix.RotationY(-leftrightRot) * Matrix.RotationX(-updownRot); //calcula la rotacion del vector de view

				movementVector = Vector3.TransformNormal(movem, Matrix.RotationY(-leftrightRot));
				var cameraFinalTarget = Vector3.TransformNormal(viewVector, cameraRotation); //direccion en que se mueve girada respecto la rotacion de la camara
				lookAt = Vector3.Add(boundPersonaje.Center, cameraFinalTarget); //vector lookAt final
			if (!flagGod)
			{

				setCamaraPrimeraPersona(lookAt);//se lo paso al setCamara
				//Actualizar valores de gravedad
				collisionManager.GravityEnabled = true;
				collisionManager.GravityForce = new Vector3(0f, 2f, 0f);

				foreach (var puerta in puertas)
				{
					animacionDePuerta(puerta);
				}


				//Mover personaje con detección de colisiones, sliding y gravedad
				//Aca se aplica toda la lógica de detección de colisiones del CollisionManager. Intenta mover el Elipsoide
				//del personaje a la posición deseada. Retorna la verdadera posicion (realMovement) a la que se pudo mover
				var realMovement = collisionManager.moveCharacter(boundPersonaje, movementVector, objetosColisionables);
			
				//personaje.move(realMovement);
				if (Input.keyDown(Key.E))
				{
					foreach (var armario in armarios)
					{
						controlDeArmario(armario);}
				}

			}
            
            
   
        }
        public override void Update()
        {
            PreUpdate();

            moverPersonaje();
            //animacionDePuerta();

            logicaDelMonstruo();

            if (Input.keyPressed(Key.G)){
                if (!flagGod)
                {
                    godMod();
				
					flagGod = true;
                }
                else {
                   //  Camara.UpdateCamera(ElapsedTime);
                    flagGod = false;
                }
		
            }

            if (Input.keyPressed(Key.C))
            {
                Clipboard.SetText(Clipboard.GetText() + String.Format(" checkpoints.Add(new Checkpoint(new Vector3({0}f, {1}f, {2}f) + origenMapa)); \n", Camara.Position.X - CheckpointHelper.origenMapa.X, 150 - CheckpointHelper.origenMapa.Y, Camara.Position.Z - CheckpointHelper.origenMapa.Z));
                CheckpointHelper.checkpoints.Add(new Checkpoint(new Vector3(Camara.Position.X, 150, Camara.Position.Z)));
            }
            //Camara.UpdateCamera(ElapsedTime);
        }

        private void renderPuerta() {
            unMesh.render();
            unMesh.BoundingBox.render();
            DrawText.drawText(unMesh.Position.ToString(), 0, 50, Color.Red);
        }
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();
            DrawText.drawText("[G]-Habilita GodMod ",0,20, Color.OrangeRed);
            DrawText.drawText("Posicion camara actual: " + TgcParserUtils.printVector3(Camara.Position), 0, 30,Color.OrangeRed);
			DrawText.drawText("armarios: " + armarios.Count.ToString(), 0, 50, Color.OrangeRed);
			DrawText.drawText("puertas " + puertas.Count.ToString(), 0, 70, Color.OrangeRed);

			//Checkpoint closestCheckpoint = CheckpointHelper.GetClosestCheckPoint(Camara.Position);

			//DrawText.drawText("Checkpoint Id: " + closestCheckpoint.id, 0, 40, Color.OrangeRed);
			//ArrowsClosesCheckPoint = CheckpointHelper.PrepareClosestCheckPoint(Camara.Position, ClosestCheckPoint, out ClosestCheckPoint);
			//ArrowsClosesCheckPoint.ForEach(a => a.render());
			//renderPuerta();
			//personaje.animateAndRender(ElapsedTime);
			// personaje.BoundingBox.render();
			// monstruo.animateAndRender(ElapsedTime);
			//for (int i = 0; i <= 24; i++) {
			//	escenario.Meshes[i].render();
			//}
			/*foreach (var mesh in escenario.Meshes)
            {
                //Renderizar modelo
                mesh.render();
                mesh.BoundingBox.render();
            }*/
			foreach (var mesh in escenario.Meshes)
			{
				//Nos ocupamos solo de las mallas habilitadas
				if (mesh.Enabled)
				{
					//Solo mostrar la malla si colisiona contra el Frustum
					var r = TgcCollisionUtils.classifyFrustumAABB(Frustum, mesh.BoundingBox);
					if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
					{
						mesh.render();
					}
				}
			}
            //Deshabilitar para que no dibuje los checkpoints en el mapa
           // CheckpointHelper.renderAll();
			//Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
			PostRender();
        }

        public override void Dispose()
        {
            escenario.disposeAll();
            personaje.Attachments.Clear();
            personaje.dispose();
        }

        public void logicaDelMonstruo()
        {

        }
    }
}