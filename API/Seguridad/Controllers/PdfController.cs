using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout;
using iText.Layout.Element;
using API.UnidadEmpleo.Application.AspiranteApp;


namespace API.Seguridad.Controllers
{
    public class PdfController : BaseApiController
    {
        [AllowAnonymous]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                success = true,
                message = "PdfController funcionando correctamente"
            });
        }

        [AllowAnonymous]
        [HttpGet("presolicitud/{curp}/{region}/{perfil}/{cuerpo}")]
        public async Task<IActionResult> Presolicitud(
            string curp,
            int region,
            int perfil,
            string cuerpo)
        {
            var plantilla = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Resources",
                "Templates",
                "PRESOLICITUD.pdf");

            var resultado = await Mediator.Send(
                new GetAspirante.Query
                {
                    Curp = curp,
                    cuerpoId = cuerpo,
                    perfil = perfil,
                    region = region
                });

            if (!resultado.IsSuccess)
            {
                return BadRequest(resultado.Error);
            }

            var aspirante = resultado.Value;

            using var memoria = new MemoryStream();

            using var reader = new PdfReader(plantilla);
            using var writer = new PdfWriter(memoria);
            using var pdf = new PdfDocument(reader, writer);

            var document = new Document(pdf);

            // COORDENADAS

            // Nombre
            float xApellidoP = 60;
            float xApellidoM = 240;
            float xNombre = 420;
            float yNombres = 660;

            // CURP
            float xCurp = 50;
            float yCurp = 620;

            // RFC
            //float xRfc = 300;
            //float yRfc = 625;

            // Fecha nacimiento
            float xDiaNacimiento = 535;
            float xMesNacimiento = 590;
            float xAnioNacimiento = 665;
            float yNacimiento = 622;

            // Número de prealta
            float xPrealta = 610;
            float yPrealta = 742;

            // Fecha solicitud
            float xDiaSolicitud = 565;
            float xMesSolicitud = 625;
            float xAnioSolicitud = 695;
            float yFechaSolicitud = 704;

            // Domicilio
            float xCalle = 60;
            float xNumero = 500;
            float yCalle = 540;

            float xEntreCalles = 55;
            float yEntreCalles = 521;

            float xColonia = 30;
            float xCP = 250;
            float xEstado = 345;
            float xMunicipio = 520;
            float yColonia = 493;

            // Celular
            float xCelular = 280;
            float yCelular = 455;
            float xCasa = 55;

            float xRecado = 565;
            float yTelefonos = 438;

        
            // ESCOLARIDAD

            float xEscolaridad = 120;
            float xDocumento = 560;
            float yEscolaridad = 387;


            // ÚLTIMO EMPLEO

            float xEmpresa = 330;
            float yEmpresa = 335;

            float xDescripcion = 60;
            float yDescripcion = 307;

            float xPuesto = 560;

            float xJefe = 110;
            float yJefe = 279;

            float xTelefonoEmpresa = 360;

            float xFechaInicio = 520;
            float xFechaFin = 650;


            // MOTIVO DE BAJA

            float xMotivo = 130;
            float yMotivo = 248;


            // POLICÍA

            float xGradoInicioPolicia = 170;
            float yGradoInicioPolicia = 219;

            float xGradoFinalPolicia = 170;
            float yGradoFinalPolicia = 205;


            // MILITAR

            float xGradoInicioMilitar = 545;
            float yGradoInicioMilitar = 219;

            float xGradoFinalMilitar = 545;
            float yGradoFinalMilitar = 205;

            // DATOS PERSONALES
        
            Escribir(document, aspirante.Apellido_Paterno ?? "", xApellidoP, yNombres, 12);
            Escribir(document, aspirante.Apellido_Materno ?? "", xApellidoM, yNombres, 12);
            Escribir(document, aspirante.Nombre ?? "", xNombre, yNombres, 12);

            Escribir(document, aspirante.Curp ?? "", xCurp, yCurp, 12);
            //Escribir(document, aspirante.Rfc ?? "", xRfc, yRfc, 11);

            // Fecha de nacimiento
            Escribir(document,
            aspirante.Fecha_Nacimiento.Day.ToString("00"),
            xDiaNacimiento,
            yNacimiento);

            Escribir(document,
            aspirante.Fecha_Nacimiento.Month.ToString("00"),
            xMesNacimiento,
            yNacimiento);

            Escribir(document,
            aspirante.Fecha_Nacimiento.Year.ToString(),
            xAnioNacimiento,
            yNacimiento);

            // DOMICILIO

            Escribir(document, aspirante.Calle ?? "", xCalle, yCalle);

            // En Aspirante se llama "numero"
            Escribir(document, aspirante.numero ?? "", xNumero, yCalle);

            Escribir(document, aspirante.EntreCalles ?? "", xEntreCalles, yEntreCalles);

            Escribir(document, aspirante.Colonia ?? "", xColonia, yColonia);

            Escribir(document,
                aspirante.CodigoPostal.ToString(),
                xCP,
                yColonia);

            Escribir(document, aspirante.Estado ?? "", xEstado, yColonia);

            Escribir(document, aspirante.Municipio ?? "", xMunicipio, yColonia);

            // TELÉFONO

            Escribir(document,
                aspirante.TelefonoCelular ?? "",
                xCelular,
                yCelular);

            document.Close();

            return File(
                memoria.ToArray(),
                "application/pdf",
                "PRESOLICITUD.pdf");
        }

        private void Escribir(
            Document doc,
            string texto,
            float x,
            float y,
            float size = 10)
        {
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            doc.ShowTextAligned(
                new Paragraph(texto)
                    .SetFont(font)
                    .SetFontSize(size),
                x,
                y,
                1,
                iText.Layout.Properties.TextAlignment.LEFT,
                iText.Layout.Properties.VerticalAlignment.BOTTOM,
                0);
        }
    }
}