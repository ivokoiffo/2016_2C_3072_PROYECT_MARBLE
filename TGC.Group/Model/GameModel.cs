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

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
	{
        private TgcScene escenario;
        private TgcSkeletalBoneAttach linterna;
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

        private Checkpoint ClosestCheckPoint;
        List<TgcArrow> ArrowsClosesCheckPoint;




        double rot = -21304;
        private bool jumping;
        double variacion;
        private float jumpingElapsedTime;
        private readonly List<Collider> objetosColisionables = new List<Collider>();

        private ElipsoidCollisionManager collisionManager;
        float larg = 4;
       
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
            personaje.Position = new Vector3(325,103.5f, 475);
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
        private void setLinterna() {
            //Crear caja como modelo de Attachment del hueos "Bip01 L Hand"
            linterna = new TgcSkeletalBoneAttach();
            //TgcTexture texturaLinterna = TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Textures\\Vegetacion\\pasto.jpg");
            //box = TgcBox.fromSize(posicionInicial, tamanioBox, pasto);
            var attachmentBox = TgcBox.fromSize(new Vector3(2, 10, 5), Color.Blue);
            linterna.Mesh = attachmentBox.toMesh("attachment");
            linterna.Bone = personaje.getBoneByName("Bip01 L Hand");
            linterna.Offset = Matrix.Translation(8, 0, -10);
            linterna.updateValues(); 
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
           // setLinterna();
            //Seteo el escenario
            escenario = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Mapa\\MPmapa+El1-TgcScene.xml");
			leftrightRot = FastMath.PI_HALF;
			updownRot = -FastMath.PI / 10.0f;
			cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);
			RotationSpeed = 0.1f;
			viewVector = new Vector3(-150,0,100);
			//initPuertaGiratoria();   
			//Almacenar volumenes de colision del escenario
			objetosColisionables.Clear();
            CollitionManager.obstaculos = new List<BoundingBoxCollider>();
            foreach (var mesh in escenario.Meshes)
            {
                objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
                CollitionManager.obstaculos.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
            }

            CheckpointHelper.BuildCheckpoints();
           

            //Crear manejador de colisiones
            collisionManager = new ElipsoidCollisionManager();
            collisionManager.GravityEnabled = true;


        }
 
        private void godMod() {
            Camara = new CamaraGod(personaje.Position,Input);
        }

        private void animacionDePuerta() {
            //Capturar Input Mouse
            if (Input.keyPressed(Key.U))
            {
                //Como ejemplo podemos hacer un movimiento simple de la cámara.
                //En este caso le sumamos un valor en Y
                ///Camara.SetCamera(Camara.Position + new Vector3(0, 10f, 0), Camara.LookAt);
                //Ver ejemplos de cámara para otras operaciones posibles.
                unMesh.Position = new Vector3(0, 0, 0);
                unMesh.Rotation = new Vector3(0, System.Convert.ToSingle(rot), 0);
                unMesh.move(new Vector3(System.Convert.ToSingle((larg - (Math.Cos(rot + 3.14) * larg))), 0, System.Convert.ToSingle(Math.Sin(rot + 3.14) * larg)));

                //Si superamos cierto Y volvemos a la posición original.
                //if (Camara.Position.Y > 300f)
                // {
                //     Camara.SetCamera(new Vector3(Camara.Position.X, 0f, Camara.Position.Z), Camara.LookAt);
                //  }
            }
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
            unMesh.Position = new Vector3(0, 0, 0);
            unMesh.Rotation = new Vector3(0, ang, 0);
            unMesh.move(new Vector3(System.Convert.ToSingle((larg - (Math.Cos(ang) * larg))), 0, System.Convert.ToSingle(Math.Sin(ang) * larg)));
        }
        private void setCamaraPrimeraPersona(Vector3 lookAt) {
            Vector3 posicionConOffset = Vector3.Add(new Vector3(5,20,2),(boundPersonaje.Center));
            Camara.SetCamera(posicionConOffset,lookAt);
        }
        private void moverPersonaje() {
            //seteo de velocidades
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
            if (Input.keyDown(Key.W))
            {
                moveForward = -velocidadCaminar;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = velocidadCaminar;
                moving = true;
        		marchaAtras = true;

			}

         

          

            else if (moving)
            {
                //Activar animacion de caminando
                personaje.playAnimation("Walk", true);
            }
            //Si no se esta moviendo ni saltando, activar animacion de Parado
            else
            {
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
                movem = new Vector3(
					FastMath.Sin(moveForward)*velocidadCaminar,
                    jump,
					FastMath.Cos(moveForward)*velocidadCaminar
                    );
				//Se actualiza matrix de rotacion, para no hacer este calculo cada vez y solo cuando en verdad es necesario.
				//if(!marchaAtras) viewVector = movementVector; //Solo cambia el vector de view si no esta caminando para atras
			}
			//maximos para los giros del vectorDeView
			if (-1f < updownRotPrevius && updownRotPrevius < 1f) { updownRot += Input.YposRelative * RotationSpeed; }

				cameraRotation = Matrix.RotationY(-leftrightRot) * Matrix.RotationX(-updownRot); //calcula la rotacion del vector de view

				movementVector = Vector3.TransformNormal(movem, Matrix.RotationY(-leftrightRot));
				var cameraFinalTarget = Vector3.TransformNormal(viewVector, cameraRotation); //direccion en que se mueve girada respecto la rotacion de la camara
				Vector3 lookAt = Vector3.Add(boundPersonaje.Center, cameraFinalTarget); //vector lookAt final
			if (!flagGod)
			{

				setCamaraPrimeraPersona(lookAt);//se lo paso al setCamara

				//Actualizar valores de gravedad
				collisionManager.GravityEnabled = true;
				collisionManager.GravityForce = new Vector3(0f, 2f, 0f);



				//Mover personaje con detección de colisiones, sliding y gravedad
				//Aca se aplica toda la lógica de detección de colisiones del CollisionManager. Intenta mover el Elipsoide
				//del personaje a la posición deseada. Retorna la verdadera posicion (realMovement) a la que se pudo mover
				var realMovement = collisionManager.moveCharacter(boundPersonaje, movementVector, objetosColisionables);
				personaje.move(realMovement);
			}
            
            
            /*
            //Si estaba saltando y hubo colision de una superficie que mira hacia abajo, desactivar salto
            if (jumping && collisionManager.Result.collisionNormal.Y < 0)
            {
                jumping = false;
            }
            */
            /*
            //Actualizar valores de normal de colision
            if (collisionManager.Result.collisionFound)
            {
                collisionNormalArrow.PStart = collisionManager.Result.collisionPoint;
                collisionNormalArrow.PEnd = collisionManager.Result.collisionPoint +
                                            Vector3.Multiply(collisionManager.Result.collisionNormal, 80);

                collisionNormalArrow.updateValues();


                collisionPoint.Position = collisionManager.Result.collisionPoint;
                collisionPoint.updateValues();

            }*/
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
            Checkpoint closestCheckpoint = CheckpointHelper.GetClosestCheckPoint(Camara.Position);
           
            DrawText.drawText("Checkpoint Id: " + closestCheckpoint.id, 0, 40, Color.OrangeRed);
            ArrowsClosesCheckPoint = CheckpointHelper.PrepareClosestCheckPoint(Camara.Position, ClosestCheckPoint, out ClosestCheckPoint);
            ArrowsClosesCheckPoint.ForEach(a => a.render());
            //renderPuerta();
            //personaje.animateAndRender(ElapsedTime);
            personaje.BoundingBox.render();
            monstruo.animateAndRender(ElapsedTime);
			//for (int i = 0; i <= 10; i++) {
			//	escenario.Meshes[i].render();
			//}
			foreach (var mesh in escenario.Meshes)
            {
                //Renderizar modelo
                mesh.render();
                mesh.BoundingBox.render();
            }

            //Deshabilitar para que no dibuje los checkpoints en el mapa
            CheckpointHelper.renderAll();
            /*linterna.Mesh.Enabled = true;
            personaje.Attachments.Add(linterna);
            */
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