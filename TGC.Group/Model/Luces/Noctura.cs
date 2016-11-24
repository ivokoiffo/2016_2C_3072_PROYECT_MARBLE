using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Utils;

namespace TGC.Group.Model
{
    class Nocturna : Luz
    {
        public Nocturna()
        {
            VelocidadConsumo = 0.009f;
            setMaximaEnergia();
        }
        public Nocturna(float velocidadConsumo)
        {
            VelocidadConsumo = velocidadConsumo;
            setMaximaEnergia();
        }
        public override void aplicarEfecto(TgcMesh mesh, Vector3 posicionCamara, Vector3 direccionLuz)
        {
            
        }

        public override string getNombreYEnergia()
        {
            return "Energia de Nocturna es de: " + (Energia+0.55f)*100;
        }
        public override float getConversion()
        {
            return 1f;
        }

        public override void setMaximaEnergia()
        {
            Energia = 0.45f;
        }

        public override string descripcion()
        {
            return "Nocturna";
        }


        private void visionNoctura()
        {

            // dibujo la escena una textura
            otroEfecto.Technique = "DefaultTechnique";
            // guardo el Render target anterior y seteo la textura como render target
            var pOldRT = D3DDevice.Instance.Device.GetRenderTarget(0);
            var pSurf = g_pRenderTarget.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pSurf);
            // hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
            var pOldDS = D3DDevice.Instance.Device.DepthStencilSurface;
            D3DDevice.Instance.Device.DepthStencilSurface = g_pDepthStencil;
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            //Dibujamos todos los meshes del escenario
            renderScene("DefaultTechnique");
            //Render personames enemigos

            D3DDevice.Instance.Device.EndScene();

            pSurf.Dispose();

            // dibujo el glow map
            otroEfecto.Technique = "DefaultTechnique";
            pSurf = g_pGlowMap.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pSurf);
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            D3DDevice.Instance.Device.BeginScene();

            //Dibujamos SOLO los meshes que tienen glow brillantes
            //Render personaje brillante
            //Render personames enemigos
            monstruo.render();
            // El resto opacos
            renderScene("DibujarObjetosOscuros");

            D3DDevice.Instance.Device.EndScene();

            pSurf.Dispose();

            // Hago un blur sobre el glow map
            // 1er pasada: downfilter x 4
            // -----------------------------------------------------
            pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
            D3DDevice.Instance.Device.SetRenderTarget(0, pSurf);

            D3DDevice.Instance.Device.BeginScene();
            otroEfecto.Technique = "DownFilter4";
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionTextured.Format;
            D3DDevice.Instance.Device.SetStreamSource(0, g_pVBV3D, 0);
            otroEfecto.SetValue("g_RenderTarget", g_pGlowMap);

            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            otroEfecto.Begin(FX.None);
            otroEfecto.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            otroEfecto.EndPass();
            otroEfecto.End();
            pSurf.Dispose();

            D3DDevice.Instance.Device.EndScene();

            D3DDevice.Instance.Device.DepthStencilSurface = pOldDS;

            // Pasadas de blur
            for (var P = 0; P < cant_pasadas; ++P)
            {
                // Gaussian blur Horizontal
                // -----------------------------------------------------
                pSurf = g_pRenderTarget4Aux.GetSurfaceLevel(0);
                D3DDevice.Instance.Device.SetRenderTarget(0, pSurf);
                // dibujo el quad pp dicho :

                D3DDevice.Instance.Device.BeginScene();
                otroEfecto.Technique = "GaussianBlurSeparable";
                D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionTextured.Format;
                D3DDevice.Instance.Device.SetStreamSource(0, g_pVBV3D, 0);
                otroEfecto.SetValue("g_RenderTarget", g_pRenderTarget4);

                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                otroEfecto.Begin(FX.None);
                otroEfecto.BeginPass(0);
                D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                otroEfecto.EndPass();
                otroEfecto.End();
                pSurf.Dispose();

                D3DDevice.Instance.Device.EndScene();

                pSurf = g_pRenderTarget4.GetSurfaceLevel(0);
                D3DDevice.Instance.Device.SetRenderTarget(0, pSurf);
                pSurf.Dispose();

                //  Gaussian blur Vertical
                // -----------------------------------------------------

                D3DDevice.Instance.Device.BeginScene();
                otroEfecto.Technique = "GaussianBlurSeparable";
                D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionTextured.Format;
                D3DDevice.Instance.Device.SetStreamSource(0, g_pVBV3D, 0);
                otroEfecto.SetValue("g_RenderTarget", g_pRenderTarget4Aux);

                D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                otroEfecto.Begin(FX.None);
                otroEfecto.BeginPass(1);
                D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                otroEfecto.EndPass();
                otroEfecto.End();

                D3DDevice.Instance.Device.EndScene();
            }

            //  To Gray Scale
            // -----------------------------------------------------
            // Ultima pasada vertical va sobre la pantalla pp dicha
            D3DDevice.Instance.Device.SetRenderTarget(0, pOldRT);
            //pSurf = g_pRenderTarget4Aux.GetSurfaceLevel(0);
            //device.SetRenderTarget(0, pSurf);

            D3DDevice.Instance.Device.BeginScene();

            otroEfecto.Technique = "GrayScale";
            D3DDevice.Instance.Device.VertexFormat = CustomVertex.PositionTextured.Format;
            D3DDevice.Instance.Device.SetStreamSource(0, g_pVBV3D, 0);
            otroEfecto.SetValue("g_RenderTarget", g_pRenderTarget);
            otroEfecto.SetValue("g_GlowMap", g_pRenderTarget4Aux);
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            otroEfecto.Begin(FX.None);
            otroEfecto.BeginPass(0);
            D3DDevice.Instance.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            otroEfecto.EndPass();
            otroEfecto.End();

            D3DDevice.Instance.Device.EndScene();

        }
    }
}
