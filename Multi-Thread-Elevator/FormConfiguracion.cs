using System;
using System.Windows.Forms;

namespace Multi_Thread_Elevator
{
    public partial class FormConfiguracion : Form
    {
        public int CantidadEdificios { get; private set; }
        public int AscensoresPorEdificio { get; private set; }

        public FormConfiguracion()
        {
            InitializeComponent();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtEdificios.Text, out int edificios) &&
                int.TryParse(txtAscensores.Text, out int ascensores) &&
                edificios > 0 && ascensores > 0)
            {
                CantidadEdificios = edificios;
                AscensoresPorEdificio = ascensores;
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ingrese valores válidos mayores que 0.");
            }
        }

    }
}
