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

                if (!Directory.Exists(pasta))
                {
                    Directory.CreateDirectory(pasta);
                }

                var caminho = Path.Combine(pasta, arquivo.FileName);

                using (var stream = new FileStream(caminho, FileMode.Create))
                {
                    arquivo.CopyTo(stream);
                }

                novoLivro.Imagem = arquivo.FileName;

            }
            else
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

            return LocalRedirect("/Livro/Cadastro");

        }

        [Route("Editar/(id)")]

        public IActionResult Editar(int id)
        {

            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            Livro livroEncontrado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;
            var categoriasDoLivroEncontrado = context.LivroCategoria.Where(identificadorLivro =>
            identificadorLivro.LivroID == id).Select(livro => livro.Categoria).ToList();

            ViewBag.Livro = livroEncontrado;
            ViewBag.Categoria = categoriasDoLivroEncontrado;

            ViewBag.CategoriasDoSistema = context.Categoria.ToList();

            return View();
        }

        [Route("Atualizar/id")]

        public IActionResult Atualizar(IFormCollection form, int id, IFormFile imagem)
        {
            Livro livroAtualizado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

            livroAtualizado.Nome = form["Nome"];
            livroAtualizado.Escritor = form["Escritor"];
            livroAtualizado.Editora = form["Editora"];
            livroAtualizado.Idioma = form["Idioma"];
            livroAtualizado.Descricao = form["Descricao"];

            if (imagem.Length > 0)
            {
                var caminhoImagem = Path.Combine("wwwroot/images/Livros", imagem.FileName);

                if (!string.IsNullOrEmpty(livroAtualizado.Imagem))
                {
                    var caminhoImagemAntiga = Path.Combine("wwwroot/images/Livros", livroAtualizado.Imagem);


                    if (System.IO.File.Exists(caminhoImagemAntiga))
                    {
                        System.IO.File.Delete(caminhoImagemAntiga);
                    }
                }

                using (var stream = new FileStream(caminhoImagem, FileMode.Create))
                {
                    imagem.CopyTo(stream);

                }

                livroAtualizado.Imagem = imagem.FileName;

                //1- Pegar as categorias selecionadas do usuario 
                var categoriasSelecionadas = form[" Categoria"].ToList();

                //2- Pegar as categorias novas do livro
                var categoriasAtuais = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();

                //3- Removeremos as categorias antigas
                foreach (var categoria in categoriasAtuais)
                {
                    if (categoriasSelecionadas.Contains(categoria.CategoriaID.ToString()))
                    {
                        context.LivroCategoria.Remove(categoria);
                    }
                }

                //4- Adicionar as novas categorias
                foreach (var categoria in categoriasSelecionadas)
                {
                    if (!categoriasAtuais.Any(c => c.CategoriaID.ToString() == categoria))
                    {
                        context.LivroCategoria.Add(new LivroCategoria { LivroID = id, CategoriaID = int.Parse(categoria) });
                    }
                }


            }

            context.SaveChanges();
            return LocalRedirect("/Livro");

        }
    
    
    }
    
    [Route("Excluir")]

        public IActionResult Excluir(int id)
        {

            Livro livroEncontrado = context.Livro.First(livro => livro.LivroID == id);

            var categoriaDoLivro = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();

            foreach (var categoria in categoriaDoLivro)
            {
                context.LivroCategoria.Remove(categoria);
            }

            context.Livro.Remove(livroEncontrado);
            context.SaveChanges();

            return LocalRedirect("/Livro");
        }
    


    }

    

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    
