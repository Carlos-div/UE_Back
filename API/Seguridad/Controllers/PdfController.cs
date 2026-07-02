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
            float xCurp = 60;
            float yCurp = 625;

            // RFC
            float xRfc = 300;
            float yRfc = 625;

            // Domicilio
            float xCalle = 60;
            float xNumero = 520;
            float yCalle = 540;

            float xEntreCalles = 60;
            float yEntreCalles = 515;

            float xColonia = 60;
            float xEstado = 360;
            float xMunicipio = 520;
            float xCP = 280;
            float yColonia = 490;

            // Celular
            float xCelular = 280;
            float yCelular = 455;

            // DATOS PERSONALES
        
            Escribir(document, aspirante.Apellido_Paterno ?? "", xApellidoP, yNombres, 12);
            Escribir(document, aspirante.Apellido_Materno ?? "", xApellidoM, yNombres, 12);
            Escribir(document, aspirante.Nombre ?? "", xNombre, yNombres, 12);

            Escribir(document, aspirante.Curp ?? "", xCurp, yCurp, 11);
            Escribir(document, aspirante.Rfc ?? "", xRfc, yRfc, 11);

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