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

            var document = new iText.Layout.Document(pdf);

               float xApellidoP = 80;
               float xApellidoM = 270;
               float xNombre    = 460;

               float yNombres = 660;
            
        
            Escribir(document, aspirante.Apellido_Paterno, xApellidoP, yNombres, 12);
            Escribir(document, aspirante.Apellido_Materno, xApellidoM, yNombres, 12);
            Escribir(document, aspirante.Nombre, xNombre, yNombres, 12);

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