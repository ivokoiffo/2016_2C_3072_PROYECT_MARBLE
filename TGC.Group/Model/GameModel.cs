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
using TGC.Core.Sound;
using System.Text.RegularExpressions;
using static TGC.Group.Model.ElipsoidCollisionManager;
using System.Linq;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
    {
        #region inicializar
        private TgcScene escenario;
        private TgcSkeletalMesh personaje;
        private TgcBoundingElipsoid boundPersonaje;
        private TgcBoundingElipsoid boundMonstruo;
        private TgcSkeletalMesh monstruo;
        private CustomSprite barra;
        private float escalaActual = 0.45f;
        private bool escondido=false;
        private bool persecucion = false;
        private bool finDePartida = false;
        private CustomSprite energia;
        private CustomSprite pantallaNegra;
        private Drawer2D drawer2D;
        private List<BoundingBoxCollider> objetosRecarga = new List<BoundingBoxCollider>();
        private List<TgcMesh> meshEscenario = new List<TgcMesh>();
        private List<TgcMesh> meshRecarga = new List<TgcMesh>();
        private Luz luz;
        private bool flagGod = false;
        private Matrix cameraRotation;
        private float leftrightRot;
        private CamaraGod camaraGod;
        private float updownRot;

        private TgcMp3Player mp3Player;
        private string currentFile;
        private TgcStaticSound sound;
        public float RotationSpeed { get; set; }
        private Vector3 viewVector;
        Vector3 lookAt;
        private Vector3 direccionLookAt;
        double rot = 0;
        double variacion;
        private CustomSprite menu;
        private readonly List<Collider> objetosColisionables = new List<Collider>();
        private readonly List<Collider> armarios = new List<Collider>();
        private readonly List<Collider> puertas = new List<Collider>();
        private ElipsoidCollisionManager colisionadorMonstruo;
        private ElipsoidCollisionManager collisionManager;
        float larg = 4;
        private Vector3 vectorOffset = new Vector3(0, 30, 0);
        private Vector3 direccionDePersecucion = new Vector3(0, 0, 0);
        private bool colisionoMonstruoEnPersecucion = false;
        float anguloAnterior = (float)Math.PI;
        private Checkpoint DestinoMonstruo { get; set; }
        private bool avanzaPositivamente = true;
        private bool estaEnMenu = true;
        #region seteosVelocidades
        private float velocidadMonstruo = 100f;
        //private float velocidadMonstruo = 200f;

        private float velocidadPersonaje = 405f;
        //private float velocidadPersonaje = 95f;

        #endregion

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
        #endregion
        private void seteoDePersonaje()
        {
            //Cargar personaje con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            personaje =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\Personaje\\personaje-TgcSkeletalMesh.xml",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\Animations\\Walk-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\Animations\\StandBy-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\Animations\\Run-TgcSkeletalAnim.xml"
                    });

            personaje.AutoTransformEnable = true;
            personaje.Scale = new Vector3(1f, 1f, 1f);
            //INICIO DEL PERSONAJE COMENTADO
            personaje.Position = new Vector3(1269f, 79f, -354f);
            personaje.rotateY(Geometry.DegreeToRadian(180f));
            boundPersonaje = new TgcBoundingElipsoid(personaje.BoundingBox.calculateBoxCenter(), personaje.BoundingBox.calculateAxisRadius());
        }
        private void seteoDelMonstruo()
        {
            //Cargar mesh y animaciones
            var loader = new TgcSkeletalLoader();
            monstruo = loader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\Monstruo\\monstruo-TgcSkeletalMesh.xml",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\Animations\\Walk-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\Animations\\StandBy-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\Animations\\Run-TgcSkeletalAnim.xml"
                    });

            monstruo.AutoTransformEnable = true;
            monstruo.Position = new Vector3(1208f, 82, 518f);
            //Escalamos el personaje 
            monstruo.Scale = new Vector3(1.5f, 1f, 1f);
            
            monstruo.playAnimation("StandBy", true);
            boundMonstruo = new TgcBoundingElipsoid(monstruo.BoundingBox.calculateBoxCenter(), monstruo.BoundingBox.calculateAxisRadius());


        }
        public override void Init()
        {
            //Para la creacion de checkpoints, borrar en el futuro
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            Clipboard.Clear();
            this.inicializarCamara();
            //Seteo el personaje
            seteoDePersonaje();
            //Seteo del monsturo
            seteoDelMonstruo();
            //Seteo el escenario
            escenario = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Mapa\\mapaProjectMarble-TgcScene.xml");

            //initPuertaGiratoria();   
            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            CollisionManager.obstaculos = new List<BoundingBoxCollider>();
            foreach (var mesh in escenario.Meshes)
            {

                if (mesh.Name.Contains("Recarga"))
                {
                    BoundingBoxCollider recarga = BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox);
                    string[] nombre = Regex.Split(mesh.Name, "-");
                    recarga.nombre = nombre[1];
                    recarga.mesh = mesh;
                    objetosRecarga.Add(recarga);
                }
                else
                {
                    if (mesh.Name.Contains("Puerta"))
                    {
                        mesh.AutoTransformEnable = true;
                        BoundingBoxCollider puerta = BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox);
                        puertas.Add(puerta);
                        
                    }
                    if (mesh.Name.Contains("Placard") || mesh.Name.Contains("Locker"))
                    {
                        armarios.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
                    }
                    BoundingBoxCollider obj = BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox);
                    obj.nombre = mesh.Name;
                    objetosColisionables.Add(obj);
                    CollisionManager.obstaculos.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
                }
                meshEscenario.Add(mesh);
            }

            CheckpointHelper.BuildCheckpoints();
            DestinoMonstruo = CheckpointHelper.checkpoints[0];

            //Crear manejador de colisiones
            collisionManager = new ElipsoidCollisionManager();
            collisionManager.SlideFactor = 2;
            collisionManager.GravityEnabled = false;
            colisionadorMonstruo = new ElipsoidCollisionManager();
            colisionadorMonstruo.SlideFactor = 2;
            colisionadorMonstruo.GravityEnabled = false;
            drawer2D = new Drawer2D();
            this.incializarMenu();
            inicializarPantallaNegra();
            mp3Player = new TgcMp3Player();
            inicializarBarra();
            luz = new Linterna();
        }
        private void incializarMenu()
        {
            menu = new CustomSprite();
            menu.Bitmap = new CustomBitmap(MediaDir + "\\menuInicio.png", D3DDevice.Instance.Device);
            var textureSize = menu.Bitmap.Size;
            menu.Position = new Vector2(0, 0);
            menu.Scaling = new Vector2((float)D3DDevice.Instance.Width / textureSize.Width, (float)D3DDevice.Instance.Height / textureSize.Height + 0.01f);

        }
        private void inicializarBarra()
        {
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
            energia.Position = new Vector2(22 + barra.Position.X, 16 + barra.Position.Y);
        }
        private void inicializarPantallaNegra() {
            pantallaNegra = new CustomSprite();
            pantallaNegra.Bitmap = new CustomBitmap(MediaDir + "\\pantallaNegra.png", D3DDevice.Instance.Device);
            var textureSize = pantallaNegra.Bitmap.Size;
            pantallaNegra.Position = new Vector2(0, 0);
            pantallaNegra.Scaling = new Vector2((float)D3DDevice.Instance.Width / textureSize.Width, (float)D3DDevice.Instance.Height / textureSize.Height);
        }
        private void inicializarCamara()
        {
            var d3dDevice = D3DDevice.Instance.Device;
            leftrightRot = FastMath.PI_HALF;
            updownRot = -FastMath.PI / 10.0f;
            cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);
            Cursor.Position = new Point(D3DDevice.Instance.Device.Viewport.Width / 2, D3DDevice.Instance.Device.Viewport.Height / 2);
            RotationSpeed = 0.1f;
            viewVector = new Vector3(1, 0, 0);
        }

        private void godMod()
        {
            camaraGod = new CamaraGod(true, boundPersonaje.Position, Input);
        }
        //OFFSET PARA PRIMERA PERSONA CON MANOS
        private Vector3 getOffset()
        {
            return personaje.Position + vectorOffset;
        }
        private void animacionDePuerta(TgcMesh unMesh)
        {

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
        private void controlDeArmario(Collider mesh)
        {
            if ((boundPersonaje.Center - mesh.BoundingSphere.Center).Length() < (boundPersonaje.Radius.Length() + mesh.BoundingSphere.Radius))
            {
                escondido = true;
                viewVector = new Vector3(-1,0,0);
                Camara.SetCamera(mesh.BoundingSphere.Center + new Vector3(10,10,10) , lookAt * -1);
            }
        }

        private float epsilon = 40f;
        private void controlDePuerta(Collider puerta)
        {
            if ((boundPersonaje.Center - puerta.BoundingSphere.Center).Length() < (boundPersonaje.Radius.Length() + puerta.BoundingSphere.Radius))
            {
                //Traslado automático
                Vector3 pos = boundPersonaje.Center - puerta.BoundingSphere.Center;
                Vector3 traslado = new Vector3();
                if (pos.Z > 0 && pos.X < epsilon && pos.X > 0)
                {
                    //resto
                    traslado.Z = puerta.BoundingSphere.Center.Z - 30f;
                    traslado.X = personaje.Position.X;
                }
                else if (pos.Z == 0)
                {
                    traslado.Z = personaje.Position.Z;
                    if (pos.X < 0) {
                        //sumo
                        traslado.X = puerta.BoundingSphere.Center.X + 30f;
                    } else {
                        //resto
                        traslado.X = puerta.BoundingSphere.Center.X - 30f;
                    }
                }
                else {
                    //resto
                    traslado.Z = puerta.BoundingSphere.Center.Z + 30f;
                    traslado.X = personaje.Position.X;

                }
                traslado.Y = personaje.Position.Y;
                personaje.Position = traslado;
                boundPersonaje.setValues(personaje.BoundingBox.calculateBoxCenter(), personaje.BoundingBox.calculateAxisRadius());
            }
        }
        private void cargarSonido(string filePath)
        {
            filePath = MediaDir + filePath;
            if (currentFile == null || currentFile != filePath)
            {
                currentFile = filePath;

                //Borrar sonido anterior
                if (sound != null)
                {
                    sound.dispose();
                    sound = null;
                }

                //Cargar sonido
                sound = new TgcStaticSound();
                sound.loadSound(currentFile, DirectSound.DsDevice);
            }
        }
        public void moverPersonaje() 
        {
            if (!flagGod)
            {


                var moveVector = new Vector3(0, 0, 0);
                var moving = false;
                if (!escondido)
                {
                    if (Input.keyDown(Key.W))
                    {
                        moving = true;
                        moveVector += new Vector3(1, 0, 0) * velocidadPersonaje;
                    }

                    if (Input.keyDown(Key.D))
                    {
                        moving = true;
                        moveVector += new Vector3(0, 0, -1) * velocidadPersonaje;
                    }

                    if (Input.keyDown(Key.A))
                    {
                        moving = true;
                        moveVector += new Vector3(0, 0, 1) * velocidadPersonaje;
                    }

                    if (Input.keyDown(Key.S))
                    {
                        moving = true;
                        moveVector += new Vector3(-1, 0, 0) * velocidadPersonaje;
                    }
                    if (moving)
                    {
                        sound.play(false);
                    }
                }
                //Vector de movimiento
                var movementVector = Vector3.Empty;
                var leftrightRotPrevius = leftrightRot - Input.XposRelative * RotationSpeed;
                var updownRotPrevius = updownRot + Input.YposRelative * RotationSpeed;
                leftrightRot -= Input.XposRelative * RotationSpeed;
                personaje.rotateY(Input.XposRelative * RotationSpeed);

                //maximos para los giros del vectorDeView
                if (-1f < updownRotPrevius && updownRotPrevius < 1f) { updownRot += Input.YposRelative * RotationSpeed; }

                cameraRotation = Matrix.RotationY(-leftrightRot) * Matrix.RotationX(-updownRot); //calcula la rotacion del vector de view

                movementVector = Vector3.TransformNormal(moveVector, Matrix.RotationY(-leftrightRot));

                direccionLookAt = Vector3.TransformNormal(viewVector, cameraRotation); //direccion en que se mueve girada respecto la rotacion de la camara
                if (!escondido)
                {
                    var realMovement = collisionManager.moveCharacter(boundPersonaje, movementVector * ElapsedTime, objetosColisionables);
                    personaje.move(realMovement);
                
                    lookAt = Vector3.Add(getOffset(), direccionLookAt); //vector lookAt final

                    Camara.SetCamera(getOffset(), lookAt);
                }
                if (Input.keyPressed(Key.P))
                {
                    foreach (var puerta in puertas)
                    {
                        controlDePuerta(puerta);
                    }
                }
                this.getColisionContraObjetoCarga();
            }
            if (!flagGod)
            {
                luz.consumir(ElapsedTime);
            }
        }

        private void getColisionContraObjetoCarga()
        {
            BoundingBoxCollider re = new BoundingBoxCollider();
            foreach (BoundingBoxCollider recarga in objetosRecarga)
            {
                if ((boundPersonaje.Center - recarga.BoundingSphere.Center).Length() < (boundPersonaje.Radius.Length() + recarga.BoundingSphere.Radius))
                {
                    re = recarga;
                    if (recarga.nombre == luz.descripcion())
                    {
                        luz.tiempoAcumulado = 0;
                        luz.setMaximaEnergia();

                    }
                    else
                    {
                        switch (recarga.nombre)
                        {
                            case "Vela":
                                luz = new Vela();
                                break;
                            case "Linterna":
                                luz = new Linterna();
                                break;
                            case "Faro":
                                luz = new Faro();
                                break;
                        }
                    }
                    meshEscenario.Remove(recarga.mesh);
                }
            }
            objetosRecarga.Remove(re);
        }

        private void ponerPantallaEnNegro() {
            drawer2D.BeginDrawSprite();
            drawer2D.DrawSprite(pantallaNegra);
            drawer2D.EndDrawSprite();
        }

        
        public override void Update()
        {
            PreUpdate();
            if (!estaEnMenu && !finDePartida)
            {
                if (Input.keyPressed(Key.G))
                {
                    if (!flagGod)
                    {
                        flagGod = true;
                        godMod();
                    }
                    else
                    {
                        flagGod = false;
                    }

                }
                if (flagGod)
                {
                    PosCamara pos = camaraGod.getPosicionGod(ElapsedTime);
                    Camara.SetCamera(pos.posicion, pos.lookAt);
                }
                else { moverPersonaje(); }
                
                Vector3 dir =  this.monstruo.Position - this.personaje.Position;
                float distanciaAPersonaje = Vector3.Length(dir);
                if (distanciaAPersonaje < 300f && !escondido)
                {
                    persecucion = true;
                    logicaPersecucion();
                }
                else
                {
                    logicaDelMonstruo();
                }
                
                finDePartida = getFinDePartida();
                if (Input.keyPressed(Key.E))
                {
                    foreach (var armario in armarios)
                    {
                        controlDeArmario(armario);
                    }
                }
                if (Input.keyUp(Key.E))
                {
                    if (escondido)
                    {
                        viewVector = new Vector3(1, 0, 0);
                        escondido = false;
                    }
                }
                actualizarEnergia();
            }
            if (estaEnMenu && Input.keyPressed(Key.Space))
            {
                estaEnMenu = false;
                cargarSonido("pisada.wav");
            }

        }

        private void actualizarEnergia()
        {
            escalaActual = luz.getConversionEnergiaABarra();
            energia.Scaling = new Vector2(escalaActual, 0.4f);
        }

        private void reproducirSonido(string nombre)
        {
            mp3Player.FileName = MediaDir + nombre;
            if (mp3Player.getStatus() == TgcMp3Player.States.Open)
            {
                //Reproducir MP3
                mp3Player.play(true);
            }

        }
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.

            PreRender();
            if (estaEnMenu)
            {
                drawer2D.BeginDrawSprite();
                drawer2D.DrawSprite(menu);
                drawer2D.EndDrawSprite();
                reproducirSonido("st.mp3");
            }
            if (finDePartida)
            {
                System.Drawing.Font font = new System.Drawing.Font("Arial", 15, FontStyle.Bold);
                DrawText.changeFont(font);
                DrawText.drawText("HAS PERDIDO", D3DDevice.Instance.Width / 2, D3DDevice.Instance.Height / 2, Color.OrangeRed);
            }
            if(!estaEnMenu && !finDePartida){
                //DrawText.drawText("[G]-Habilita GodMod ", 0, 20, Color.OrangeRed);
                DrawText.drawText("Posicion camara actual: " + TgcParserUtils.printVector3(Camara.Position), 0, 150, Color.Blue);
                DrawText.drawText(luz.getNombreYEnergia(), 0, 90, Color.Blue);
                drawer2D.BeginDrawSprite();

                //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
                drawer2D.DrawSprite(barra);
                drawer2D.DrawSprite(energia);
               
                //Finalizar el dibujado de Sprites
                drawer2D.EndDrawSprite();
                #region ComentoCheckPoint
                DrawText.drawText("Checkpoint Id: " + DestinoMonstruo.id, 0, 40, Color.OrangeRed);
                //boundMonstruo.render();
                //boundPersonaje.render();
                monstruo.animateAndRender(ElapsedTime);
                CheckpointHelper.renderAll();
                #endregion
                
                foreach (var mesh in meshEscenario)
                {
                    //Nos ocupamos solo de las mallas habilitadas
                    if (mesh.Enabled)
                    {
                        //Solo mostrar la malla si colisiona contra el Frustum
                        var r = TgcCollisionUtils.classifyFrustumAABB(Frustum, mesh.BoundingBox);
                        if (r != TgcCollisionUtils.FrustumResult.OUTSIDE)
                        {
                            if (flagGod) { luz.deshabilitarEfecto(mesh); }
                            else
                            {
                                luz.aplicarEfecto(mesh, Camara.Position, direccionLookAt);
                            }
                            mesh.render();

                        }
                    }
                }
              
            }
            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }
       

        public override void Dispose()
        {
            escenario.disposeAll();
            personaje.dispose();
            boundPersonaje.dispose();
            boundMonstruo.dispose();
        }
        private bool getFinDePartida()
        {
            return (boundPersonaje.Center - boundMonstruo.Center).Length() < (boundPersonaje.Radius.Length() + boundMonstruo.Radius.Length());
        }
        
        public void logicaPersecucion()
        {
            if (!colisionoMonstruoEnPersecucion) {
                direccionDePersecucion = this.personaje.Position - this.monstruo.Position;
                direccionDePersecucion = new Vector3(direccionDePersecucion.X, 0f, direccionDePersecucion.Z);
                direccionDePersecucion.Normalize();
            }
            var realMovement = colisionadorMonstruo.moveCharacter(boundMonstruo, direccionDePersecucion * ElapsedTime*velocidadMonstruo, objetosColisionables);
            monstruo.move(realMovement);
            cargarSonido("grito.wav");
            sound.play();
            mp3Player.pause();
            monstruo.playAnimation("Run", true);
            float angulo = (float)Math.Atan2(-direccionDePersecucion.X, direccionDePersecucion.Z);
            monstruo.rotateY(anguloAnterior - angulo);
            anguloAnterior = angulo;
            if (realMovement == new Vector3(0, 0, 0))
            {
                colisionoMonstruoEnPersecucion = true;
                float unAngulo = (float)Math.PI / 2;
                direccionDePersecucion = new Vector3((float)Math.Cos(unAngulo + monstruo.Rotation.Y), 0, (float)Math.Sin(unAngulo + monstruo.Rotation.Y));
                direccionDePersecucion.Normalize();
            }
            else { colisionoMonstruoEnPersecucion = false; }
        }
        
        public void logicaDelMonstruo(){
            cargarSonido("pisada.wav");
            mp3Player.resume();
            if (persecucion) {
                Vector3 pos = CheckpointHelper.checkpoints[0].Position;
                pos.Y =  monstruo.Position.Y;
                monstruo.Position = pos;
                boundMonstruo.setValues(monstruo.BoundingBox.calculateBoxCenter(), monstruo.BoundingBox.calculateAxisRadius());
                avanzaPositivamente = true;
                persecucion = false;
                DestinoMonstruo = CheckpointHelper.checkpoints[0];
            }
            else
            {
                float distanciaAlCheckPointDestino = Vector3.Length(DestinoMonstruo.Position - monstruo.Position);
                bool rotacionPorCambioDeDestino = false;
                if (distanciaAlCheckPointDestino < 100f)
                {
                    //Intercambio de checkpoint
                    int actual = CheckpointHelper.checkpoints.FindIndex(c => c == DestinoMonstruo);
                    int proxPos = actual;

                    if (actual == CheckpointHelper.checkpoints.Count - 1 && avanzaPositivamente)
                    {
                        avanzaPositivamente = false;

                    }
                    if (actual == 0 && !avanzaPositivamente) avanzaPositivamente = true;

                    if (avanzaPositivamente) { proxPos++; } else { proxPos--; }

                    var siguienteCheckPoint = CheckpointHelper.checkpoints[proxPos];

                    DestinoMonstruo = siguienteCheckPoint;
                    rotacionPorCambioDeDestino = true;
                }
                Vector3 dir = DestinoMonstruo.Position - this.monstruo.Position;

                dir = new Vector3(dir.X, 0f, dir.Z);
                dir.Normalize();

                if (rotacionPorCambioDeDestino) {
                    float angulo = (float)Math.Atan2( -dir.X, dir.Z);
                    monstruo.rotateY(anguloAnterior - angulo);
                    anguloAnterior = angulo;
                }
                var realMovement = colisionadorMonstruo.moveCharacter(boundMonstruo, dir * ElapsedTime * velocidadMonstruo, objetosColisionables);
                monstruo.move(realMovement);
                monstruo.playAnimation("Walk", true);
            }
        }
        
    }
}