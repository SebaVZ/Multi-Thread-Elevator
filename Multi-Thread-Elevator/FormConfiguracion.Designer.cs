namespace Multi_Thread_Elevator
{
    partial class FormConfiguracion
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtEdificios;
        private TextBox txtAscensores;
        private Button btnAceptar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtEdificios = new TextBox { Location = new Point(20, 20), Width = 100 };
            txtAscensores = new TextBox { Location = new Point(20, 60), Width = 100 };
            btnAceptar = new Button { Text = "Aceptar", Location = new Point(20, 100) };
            btnAceptar.Click += btnAceptar_Click;

            Controls.Add(new Label { Text = "Edificios:", Location = new Point(130, 20) });
            Controls.Add(new Label { Text = "Ascensores por edificio:", Location = new Point(130, 60) });
            Controls.Add(txtEdificios);
            Controls.Add(txtAscensores);
            Controls.Add(btnAceptar);

            Text = "Configuración Inicial";
            Size = new Size(500, 180);
        }
    }
}
