using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bibliotec.Controllers
{
    [Route("[controller]")]
    public class LivroController : Controller
    {
        private readonly ILogger<LivroController> _logger;

        public LivroController(ILogger<LivroController> logger)
        {
            _logger = logger;
        }

        Context context = new Context();

        public IActionResult Index()
        {
            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            List<Livro> listaLivros = context.Livro.ToList();

            var livrosReservados = context.LivroReserva.ToDictionary(livro => livro.LivroID, Livror => Livror.DtReserva);

            ViewBag.Livros = listaLivros;

            ViewBag.LivrosComReserva = livrosReservados;

            return View();
        }
        // metodo que retorna a tela de cadastro

        [Route("Cadastro")]
        public IActionResult Cadastro()
        {

            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            ViewBag.Categorias = context.Categoria.ToList();

            return View();
        }

        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection form)
        {
            Livro novoLivro = new Livro();

            novoLivro.Nome = form["Nome"].ToString();
            novoLivro.Nome = form["Descricao"].ToString();
            novoLivro.Nome = form["Editora"].ToString();
            novoLivro.Nome = form["Escritor"].ToString();
            novoLivro.Nome = form["Idioma"].ToString();

            if (form.Files.Count > 0)
            {
                var arquivo = form.Files[0];

                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Livros");

                if (Directory.Exists(pasta))
                {
                    Directory.CreateDirectory(pasta);
                }

                var caminho = Path.Combine(pasta, arquivo.FileName);

                using (var stream = new FileStream(caminho, FileMode.Create))
                {
                    arquivo.CopyTo(stream);
                }

                novoLivro.Imagem = arquivo.FileName;

            } else
            {
                novoLivro.Imagem = "padrao.png";
            }

            context.Livro.Add(novoLivro);
            context.SaveChanges();

            List<LivroCategoria> listaLivroCategorias = new List<LivroCategoria>();

            string[] categoriasSelecionadas = form["Categoria"].ToString().Split(',');

            foreach (string categoria in categoriasSelecionadas)
            {
                LivroCategoria livroCategoria = new LivroCategoria();
                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novoLivro.LivroID;

                listaLivroCategorias.Add(livroCategoria);
            }



            context.LivroCategoria.AddRange(listaLivroCategorias);
            context.SaveChanges();

            return LocalRedirect("/Cadastro");

        }



        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}