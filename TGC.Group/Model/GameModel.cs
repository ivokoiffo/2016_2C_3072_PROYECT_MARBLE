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

namespace TGC.Group.Model
{
	
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
	{
        private TgcScene escenario;
        private TgcSkeletalBoneAttach linterna;
        private TgcSkeletalMesh personaje;
        private TgcBoundingElipsoid boundPersonaje;
        private TgcMesh unMesh;
        private bool flagGod = false;
        double rot = -21304;
        private bool jumping;
        double variacion;
        private float jumpingElapsedTime;
        private readonly List<Collider> objetosColisionables = new List<Collider>();

        private ElipsoidCollisionManager collisionManager;
        float larg = 4;
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

		private TgcMesh setMeshToOrigin(TgcMesh mesh)
		{
			//Desplazar los vertices del mesh para que tengan el centro del AABB en el origen
			var center = mesh.BoundingBox.calculateBoxCenter();
			mesh=moveMeshVertices(-center,mesh);

			//Ubicar el mesh en donde estaba originalmente
			mesh.BoundingBox.setExtremes(mesh.BoundingBox.PMin - center, mesh.BoundingBox.PMax - center);
			mesh.Position = center;
			return mesh;
		}

		private TgcMesh moveMeshVertices(Vector3 offset,TgcMesh mesh)
		{	
			switch (mesh.RenderType)
			{
			case TgcMesh.MeshRenderType.VERTEX_COLOR:
				var verts1 = (TgcSceneLoader.VertexColorVertex[])mesh.D3dMesh.LockVertexBuffer(
					typeof(TgcSceneLoader.VertexColorVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
				for (var i = 0; i < verts1.Length; i++)
				{
					verts1[i].Position = verts1[i].Position + offset;
				}
				mesh.D3dMesh.SetVertexBufferData(verts1, LockFlags.None);
				mesh.D3dMesh.UnlockVertexBuffer();
					return mesh;
			

			case TgcMesh.MeshRenderType.DIFFUSE_MAP:
				var verts2 = (TgcSceneLoader.DiffuseMapVertex[])mesh.D3dMesh.LockVertexBuffer(
					typeof(TgcSceneLoader.DiffuseMapVertex), LockFlags.ReadOnly, mesh.D3dMesh.NumberVertices);
				for (var i = 0; i < verts2.Length; i++)
				{
					verts2[i].Position = verts2[i].Position + offset;
				}
				mesh.D3dMesh.SetVertexBufferData(verts2, LockFlags.None);
				mesh.D3dMesh.UnlockVertexBuffer();
				return mesh;

			case TgcMesh.MeshRenderType.DIFFUSE_MAP_AND_LIGHTMAP:
				var verts3 = (TgcSceneLoader.DiffuseMapAndLightmapVertex[])mesh.D3dMesh.LockVertexBuffer(
					typeof(TgcSceneLoader.DiffuseMapAndLightmapVertex), LockFlags.ReadOnly,
					mesh.D3dMesh.NumberVertices);
				for (var i = 0; i < verts3.Length; i++)
				{
					verts3[i].Position = verts3[i].Position + offset;
				}
				mesh.D3dMesh.SetVertexBufferData(verts3, LockFlags.None);
				mesh.D3dMesh.UnlockVertexBuffer();
				return mesh;
			}
			return mesh;
		}
        //Caja que se muestra en el ejemplo.
        private TgcBox Box { get; set; }

        //Mesh de TgcLogo.
        private TgcMesh Mesh { get; set; }

        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }

        private void initPuertaGiratoria() {
            unMesh = escenario.Meshes.Find((TgcMesh obj) => obj.Name.Contains("Puerta"));
            setMeshToOrigin(unMesh);
            unMesh.Position = new Vector3(0, 0, 0);
            larg = (new Vector2(unMesh.BoundingBox.PMin.X, unMesh.BoundingBox.PMin.Z) - new Vector2(unMesh.BoundingBox.PMax.X, unMesh.BoundingBox.PMax.Z)).Length() / 2;
        }
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
            personaje.Position = new Vector3(0,-17, 0);
            //Escalamos el personaje ya que sino la escalera es demasiado grande.
            personaje.Scale = new Vector3(1.0f, 1.0f, 1.0f);
            boundPersonaje = new TgcBoundingElipsoid(personaje.BoundingBox.calculateBoxCenter() + new Vector3(0, 0, 0), new Vector3(12, 28, 12));
            jumping = false;
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
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;
            //Seteo el personaje
            seteoDePersonaje();
            setLinterna();
            //Seteo el escenario
            escenario = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Mapa\\mapa-TgcScene.xml");
            setCamaraPrimeraPersona();
            //initPuertaGiratoria();   
            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            foreach (var mesh in escenario.Meshes)
            {
                objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
            }

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
        private void setCamaraPrimeraPersona() {
            Vector3 posicionConOffset = Vector3.Add(new Vector3(8,20,0),(boundPersonaje.Center));
            Camara.SetCamera(posicionConOffset,new Vector3(0,0,0));
        }
        private void moverPersonaje() {
            //seteo de velocidades
            var velocidadCaminar = 1.0f;
            var velocidadRotacion =1.0f;
            var velocidadSalto = 1.0f;
            var tiempoSalto = 1.0f;

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;
            float jump = 0;

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
            }

            //Derecha
            if (Input.keyDown(Key.D))
            {
                rotate = velocidadRotacion;
                rotating = true;
            }

            //Izquierda
            if (Input.keyDown(Key.A))
            {
                rotate = -velocidadRotacion;
                rotating = true;
            }

            //Jump
            if (!jumping && Input.keyPressed(Key.Space))
            {
                //Se puede saltar solo si hubo colision antes
                if (collisionManager.Result.collisionFound)
                {
                    jumping = true;
                    jumpingElapsedTime = 0f;
                    jump = 0;
                }
            }

            //Si hubo rotacion
            if (rotating)
            {
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                personaje.rotateY(rotAngle);
            }

            //Saltando
            if (jumping)
            {
                //Activar animacion de saltando
                personaje.playAnimation("Jump", true);
            }
            //Si hubo desplazamiento
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

            //Actualizar salto
            if (jumping)
            {
                //El salto dura un tiempo hasta llegar a su fin
                jumpingElapsedTime += ElapsedTime;
                if (jumpingElapsedTime > tiempoSalto)
                {
                    jumping = false;
                }
                else
                {
                    jump = velocidadSalto * (tiempoSalto - jumpingElapsedTime);
                }
            }

            //Vector de movimiento
            var movementVector = Vector3.Empty;
            if (moving || jumping)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                movementVector = new Vector3(
                    FastMath.Sin(personaje.Rotation.Y) * moveForward,
                    jump,
                    FastMath.Cos(personaje.Rotation.Y) * moveForward
                    );
            }

            //Actualizar valores de gravedad
            collisionManager.GravityEnabled = true;
            collisionManager.GravityForce = new Vector3(0f, 2f, 0f);

            //Si esta saltando, desactivar gravedad
            if (jumping)
            {
                collisionManager.GravityEnabled = false;
            }

            //Mover personaje con detección de colisiones, sliding y gravedad
                //Aca se aplica toda la lógica de detección de colisiones del CollisionManager. Intenta mover el Elipsoide
                //del personaje a la posición deseada. Retorna la verdadera posicion (realMovement) a la que se pudo mover
                var realMovement = collisionManager.moveCharacter(boundPersonaje, movementVector,objetosColisionables);
                personaje.move(realMovement);
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

            if (Input.keyPressed(Key.G)){
                if (!flagGod)
                {
                    godMod();
                    flagGod = true;
                }
                else {
                    setCamaraPrimeraPersona();
                    Camara.UpdateCamera(ElapsedTime);
                    flagGod = false;
                }
            }
            setCamaraPrimeraPersona();
            Camara.UpdateCamera(ElapsedTime);
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
      
            //renderPuerta();
            personaje.animateAndRender(ElapsedTime);
            foreach (var mesh in escenario.Meshes)
            {
                //Renderizar modelo
                mesh.render();
            }
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
    }
}