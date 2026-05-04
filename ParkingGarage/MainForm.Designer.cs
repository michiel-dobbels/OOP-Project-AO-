namespace ParkingGarage;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    private FlowLayoutPanel _menuPanel = null!;
    private Label _lblTitle = null!;
    private Label _creditLabel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _lblTitle = new Label();
        var btnStart = new Button();
        var btnExit = new Button();
        _menuPanel = new FlowLayoutPanel();
        SuspendLayout();

        _lblTitle.AutoSize = true;
        _lblTitle.Font = new Font("Segoe UI", 26F, FontStyle.Bold, GraphicsUnit.Point);
        _lblTitle.ForeColor = Color.FromArgb(255, 214, 120);
        _lblTitle.Margin = new Padding(0, 0, 0, 28);
        _lblTitle.Text = "Ondergrondse parking — simulatie";

        _creditLabel = new Label();
        _creditLabel.AutoSize = true;
        _creditLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _creditLabel.BackColor = AppColors.FieldBackground;
        _creditLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        _creditLabel.ForeColor = Color.FromArgb(130, 150, 170);
        _creditLabel.Text = "Created by Michiel Dobbels (2026)";

        btnStart.AutoSize = true;
        btnStart.BackColor = AppColors.PanelControl;
        btnStart.FlatStyle = FlatStyle.Flat;
        btnStart.FlatAppearance.BorderColor = Color.Gainsboro;
        btnStart.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        btnStart.ForeColor = Color.WhiteSmoke;
        btnStart.Margin = new Padding(8);
        btnStart.Padding = new Padding(32, 12, 32, 12);
        btnStart.Text = "Start parkeren";
        btnStart.UseVisualStyleBackColor = false;
        btnStart.Click += BtnStart_Click;

        btnExit.AutoSize = true;
        btnExit.BackColor = AppColors.PanelControl;
        btnExit.FlatStyle = FlatStyle.Flat;
        btnExit.FlatAppearance.BorderColor = Color.Gainsboro;
        btnExit.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        btnExit.ForeColor = Color.WhiteSmoke;
        btnExit.Margin = new Padding(8);
        btnExit.Padding = new Padding(32, 12, 32, 12);
        btnExit.Text = "stop simulatie";
        btnExit.UseVisualStyleBackColor = false;
        btnExit.Click += BtnExit_Click;

        _menuPanel.AutoSize = true;
        _menuPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _menuPanel.BackColor = AppColors.FieldBackground;
        _menuPanel.FlowDirection = FlowDirection.TopDown;
        _menuPanel.WrapContents = false;
        _menuPanel.Controls.Add(_lblTitle);
        _menuPanel.Controls.Add(btnStart);
        _menuPanel.Controls.Add(btnExit);
        _menuPanel.Dock = DockStyle.None;
        _menuPanel.Padding = new Padding(40);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppColors.FieldBackground;
        ClientSize = AppLayout.SimulationClientSize;
        Controls.Add(_menuPanel);
        Controls.Add(_creditLabel);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Parking — simulatie";
        ResumeLayout(false);
    }
}
