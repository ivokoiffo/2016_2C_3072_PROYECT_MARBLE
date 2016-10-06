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
        private TgcBoundingSphere boundPersonaje;
        private TgcMesh unMesh;
        private bool flagGod = false;
        double rot = -21304;
        double variacion;
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
        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        /// 
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
            personaje.AutoTransformEnable = false;
            //Configurar animacion inicial
            personaje.playAnimation("StandBy", true);
            //Escalarlo porque es muy grande
            personaje.Position = new Vector3(0,-22, 0);
            //Rotarlo 180° porque esta mirando para el otro lado
            personaje.rotateY(Geometry.DegreeToRadian(180f));
            //Escalamos el personaje ya que sino la escalera es demaciado grande.
            personaje.Scale = new Vector3(1.0f, 1.0f, 1.0f);
            personaje.UpdateMeshTransform();
            //BoundingSphere que va a usar el personaje
            personaje.AutoUpdateBoundingBox = false;
            boundPersonaje = new TgcBoundingSphere(personaje.BoundingBox.calculateBoxCenter(),personaje.BoundingBox.calculateBoxRadius());
        }
        private void setLinterna() {
            //Crear caja como modelo de Attachment del hueos "Bip01 L Hand"
            /*

            */
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
            //Suelen utilizarse objetos que manejan el comportamiento de la camara.
            //Lo que en realidad necesitamos gráficamente es una matriz de View.
            //El framework maneja una cámara estática, pero debe ser inicializada.
            //Posición de la camara.
            //initPuertaGiratoria();


        }
 
        private void godMod() {
    
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
            Camara = new TgcFpsCamera(personaje.Position,Input);
        }
        public override void Update()
        {
            PreUpdate();
            //animacionDePuerta();
            if (Input.keyPressed(Key.G)){
                if (!flagGod)
                {
                    godMod();
                    flagGod = true;
                }
                else {
                    setCamaraPrimeraPersona();
                    flagGod = false;
                }
           }
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
            linterna.Mesh.Enabled = true;
            personaje.Attachments.Add(linterna);

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