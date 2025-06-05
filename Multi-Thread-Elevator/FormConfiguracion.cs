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

            // para que al pulsar Enter en cualquier otro control con HandleKeyPreview funcione.
            //this.AcceptButton = btnAceptar;

            txtEdificios.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;   // evita el "ding"
                    txtAscensores.Focus();       // mueve foco
                }
            };

            txtAscensores.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnAceptar.PerformClick();   // dispara tu lógica de Aceptar
                }
            };
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
