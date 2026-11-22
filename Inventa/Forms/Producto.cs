using CBP.WinFrms.Notification;
using Inventa.Forms.Mantenedores;
using Inventa.Forms.Productos;
using Inventa.Infrastructure;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Inventa.Forms
{
    public partial class Producto : Form
    {
        private readonly IFormFactory _formFactory;

        public Producto(IFormFactory formFactory)
        {
            InitializeComponent();

            _formFactory = formFactory;

        }


    }
}