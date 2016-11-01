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
        private TgcBox meshRecargaLuz;
        private CustomSprite barra;
        private float escalaActual=0.45f;
        private CustomSprite energia;
        private Drawer2D drawer2D;
        private List<TgcBox> objetosRecarga = new List<TgcBox>();
        private Luz luz;
        private bool flagGod = false;
		private Matrix cameraRotation;
		private float leftrightRot;
		private float updownRot;
		public float RotationSpeed { get; set; }
		private Vector3 viewVector;
		Vector3 lookAt;
        private Vector3 direccionLookAt;
        double rot = 0;
        double variacion;

        private readonly List<Collider> objetosColisionables = new List<Collider>();
		private readonly List<Collider> armarios = new List<Collider>();
		private readonly List<TgcMesh> puertas = new List<TgcMesh>();
        private ElipsoidCollisionManager collisionManager;
        float larg = 4;
        private Vector3 vectorOffset =  new Vector3(3,30,5);
        private Checkpoint ClosestCheckPoint;
        List<TgcArrow> ArrowsClosesCheckPoint;
        private Vector3 objetive;

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

            personaje.AutoTransformEnable = true;
            personaje.Scale = new Vector3(1f, 1f, 1f);
            personaje.Position = new Vector3(325f,102f, 474f);
            personaje.rotateY(Geometry.DegreeToRadian(180f));
            boundPersonaje = new TgcBoundingElipsoid(personaje.BoundingBox.calculateBoxCenter(), personaje.BoundingBox.calculateAxisRadius());
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
            monstruo.Position = new Vector3(325, 120, 474);
            //Escalamos el personaje ya que sino la escalera es demasiado grande.
            monstruo.Scale = new Vector3(0.65f, 0.65f, 0.65f);

            monstruo.playAnimation(animationList[0], true);

            boundMonstruo = new TgcBoundingElipsoid(monstruo.BoundingBox.calculateBoxCenter(), monstruo.BoundingBox.calculateAxisRadius());


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
            viewVector = new Vector3(1, 0, 0);

            meshRecargaLuz = TgcBox.fromSize(new Vector3(10, 10, 10), Color.Red);
            meshRecargaLuz.AutoTransformEnable = true;
            meshRecargaLuz.Position = new Vector3(513.33f, 120.7675f, 595.4409f);
            objetosRecarga.Add(meshRecargaLuz);
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
                if (mesh.Name.Contains("Placard") || mesh.Name.Contains("Locker"))
                {
                    armarios.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
                }
                objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
                CollisionManager.obstaculos.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
            }

            CheckpointHelper.BuildCheckpoints();


            //Crear manejador de colisiones
            collisionManager = new ElipsoidCollisionManager();
            collisionManager.GravityEnabled = false;

            luz = new Vela();
            drawer2D = new Drawer2D();
            //Crear Sprite
            barra = new CustomSprite();
            energia = new CustomSprite();
            barra.Bitmap = new CustomBitmap(MediaDir + "\\barra.png", D3DDevice.Instance.Device);
            barra.Scaling = new Vector2(0.5f, 0.5f);
            // barraDuracion.Color = Color.Empty;
            //var textureSize = barra.Bitmap.Size;
            barra.Position = new Vector2(0, 0);
            energia.Bitmap = new CustomBitmap(MediaDir + "\\energia.png", D3DDevice.Instance.Device);
            energia.Scaling = new Vector2(escalaActual, 0.4f);
            energia.Position = new Vector2(22+barra.Position.X, 16+barra.Position.Y);
        }

        private void godMod() {
            Camara = new CamaraGod(true,personaje.Position,Input);
        }
        //OFFSET PARA PRIMERA PERSONA CON MANOS
        private Vector3 getOffset() {
            return personaje.Position + vectorOffset;
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
            var moveVector = new Vector3(0, 0, 0);
            //Calcular proxima posicion de personaje segun Input
            var moving = false;

            if (Input.keyDown(Key.W)){
                moving = true;
                moveVector += new Vector3(1, 0, 0) * velocidadCaminar;
            }
            //Strafe right
            if (Input.keyDown(Key.D))
            {
                moveVector += new Vector3(0, 0, -1) * velocidadCaminar;
            }

            //Strafe left
            if (Input.keyDown(Key.A))
            {
                moveVector += new Vector3(0, 0, 1) * velocidadCaminar;
            }

            if (Input.keyDown(Key.S))
            {
                moveVector += new Vector3(-1, 0, 0) * velocidadCaminar;
                moving = true;

			}
            if (moving){
                //Activar animacion de caminando
                personaje.playAnimation("Walk", true);
            }
            else{
                personaje.playAnimation("StandBy", true);
            }

            //Vector de movimiento
            var movementVector = Vector3.Empty;
			var leftrightRotPrevius= leftrightRot-Input.XposRelative * RotationSpeed;
			var updownRotPrevius = updownRot + Input.YposRelative * RotationSpeed;
		    leftrightRot -= Input.XposRelative * RotationSpeed; 
			personaje.rotateY(Input.XposRelative* RotationSpeed);

            //maximos para los giros del vectorDeView
            if (-1f < updownRotPrevius && updownRotPrevius < 1f) { updownRot += Input.YposRelative * RotationSpeed; }

				cameraRotation = Matrix.RotationY(-leftrightRot) * Matrix.RotationX(-updownRot); //calcula la rotacion del vector de view

				movementVector = Vector3.TransformNormal(moveVector, Matrix.RotationY(-leftrightRot));
                direccionLookAt = Vector3.TransformNormal(viewVector, cameraRotation); //direccion en que se mueve girada respecto la rotacion de la camara
			if (!flagGod)
			{
                var realMovement = collisionManager.moveCharacter(boundPersonaje, movementVector, objetosColisionables);
                personaje.move(realMovement);
                lookAt = Vector3.Add(getOffset(), direccionLookAt); //vector lookAt final

                Camara.SetCamera(getOffset(),lookAt);
                collisionManager.SlideFactor = 2;
				foreach (var puerta in puertas)
				{
					animacionDePuerta(puerta);
				}

				if (Input.keyDown(Key.E))
				{
					foreach (var armario in armarios)
					{
						controlDeArmario(armario);}
				}
                this.getColisionContraObjetoCarga();
                luz.consumir(ElapsedTime);
            }
        }

        private void getColisionContraObjetoCarga()
        {
            if ((boundPersonaje.Center - meshRecargaLuz.BoundingBox.calculateBoxCenter()).Length() < (boundPersonaje.Radius.Length() + meshRecargaLuz.BoundingBox.calculateBoxRadius()))
            {
                luz.tiempoAcumulado = 0;
                luz.setMaximaEnergia();
            }
        }

        public override void Update()
        {
            PreUpdate();

            moverPersonaje();
            //animacionDePuerta();
            
            //logicaDelMonstruo();

            if (Input.keyPressed(Key.G)){
                if (!flagGod)
                {
                    godMod();
					flagGod = true;
                }
                else {
                    flagGod = false;
                }
		
            }

            if (Input.keyPressed(Key.C))
            {
                Clipboard.SetText(Clipboard.GetText() + String.Format(" checkpoints.Add(new Checkpoint(new Vector3({0}f, {1}f, {2}f) + origenMapa)); \n", Camara.Position.X - CheckpointHelper.origenMapa.X, 150 - CheckpointHelper.origenMapa.Y, Camara.Position.Z - CheckpointHelper.origenMapa.Z));
                CheckpointHelper.checkpoints.Add(new Checkpoint(new Vector3(Camara.Position.X, 150, Camara.Position.Z)));
            }
            actualizarEnergia();
        }

        private void actualizarEnergia()
        {
            escalaActual = luz.getConversionEnergiaABarra();
            energia.Scaling = new Vector2(escalaActual, 0.4f);
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
            DrawText.drawText("Posicion camara actual: " + TgcParserUtils.printVector3(getOffset()), 0, 30,Color.OrangeRed);
            DrawText.drawText("armarios: " + armarios.Count.ToString(), 0, 50, Color.OrangeRed);
			DrawText.drawText("puertas " + puertas.Count.ToString(), 0, 70, Color.OrangeRed);
            DrawText.drawText(luz.getNombreYEnergia(), 0, 90, Color.OrangeRed);
            drawer2D.BeginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            drawer2D.DrawSprite(barra);
            drawer2D.DrawSprite(energia);
            //drawer2D.DrawLine(new Vector2(0,30), new Vector2(40,30), Color.Red, 40, false);
            //Finalizar el dibujado de Sprites
            drawer2D.EndDrawSprite();
            #region ComentoCheckPoint
            //Checkpoint closestCheckpoint = CheckpointHelper.GetClosestCheckPoint(Camara.Position);

            //DrawText.drawText("Checkpoint Id: " + closestCheckpoint.id, 0, 40, Color.OrangeRed);
            //ArrowsClosesCheckPoint = CheckpointHelper.PrepareClosestCheckPoint(Camara.Position, ClosestCheckPoint, out ClosestCheckPoint);
            //ArrowsClosesCheckPoint.ForEach(a => a.render());
            //monstruo.animateAndRender(ElapsedTime);
            //CheckpointHelper.renderAll();
            #endregion
            //renderPuerta();
            //personaje.animateAndRender(ElapsedTime);
            personaje.BoundingBox.render();
            meshRecargaLuz.render();
			
			foreach (var mesh in escenario.Meshes)
			{
				//Nos ocupamos solo de las mallas habilitadas
				if (mesh.Enabled)
				{
					//Solo mostrar la malla si colisiona contra el Frustum
					var r = TgcCollisionUtils.classifyFrustumAABB(Frustum, mesh.BoundingBox);
					if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
					{
                        if (flagGod) { luz.deshabilitarEfecto(mesh); } else {
                            luz.aplicarEfecto(mesh, getOffset(), direccionLookAt); }
						mesh.render();

                    }
				}
			}
            //Deshabilitar para que no dibuje los checkpoints en el mapa
            
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
            var monsterClosestCheckpoint = CheckpointHelper.GetClosestCheckPoint(monstruo.Position);
            var avatarClosestCheckpoint = CheckpointHelper.GetClosestCheckPoint(this.Camara.Position);
            if (monsterClosestCheckpoint.Neighbors.Contains(avatarClosestCheckpoint) ||avatarClosestCheckpoint == monsterClosestCheckpoint)
            {
                objetive = this.Camara.Position;
            }
            else
            {
                //Encontrar el algoritmo del camino más corto de un checkpoint al otro
                var nextCheckpoint = monsterClosestCheckpoint.Neighbors.Find(c => c.CanArriveTo(avatarClosestCheckpoint));
                objetive = nextCheckpoint.Position;
            }

            Vector3 dir = objetive - this.monstruo.Position;

            dir = new Vector3(dir.X, 0f, dir.Z);
            dir.Normalize();

            monstruo.move(dir * 0.7f);
        }
    }
}