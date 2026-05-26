using jmedrandas6.Modelos;
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;

namespace jmedrandas6.Views;

public partial class vEstudiante : ContentPage
{
    private const string URL = "http://192.168.100.21/ws_estudiante/post.php";
    private readonly HttpClient cliente = new HttpClient();
    private ObservableCollection<Estudiante> _estu;
    private ObservableCollection<Estudiante> _estuOriginal; // Guardar copia original
    private Estudiante _estudianteSeleccionado;

    public vEstudiante()
    {
        InitializeComponent();
        Get(); // Cargar estudiantes al abrir

        listaEstudiantes.ItemSelected += OnEstudianteSeleccionado;
    }

    // GET - Cargar estudiantes
    public async void Get()
    {
        try
        {
            var content = await cliente.GetStringAsync(URL);
            List<Estudiante> objEstudiantes = JsonConvert.DeserializeObject<List<Estudiante>>(content);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _estuOriginal = new ObservableCollection<Estudiante>(objEstudiantes);
                _estu = new ObservableCollection<Estudiante>(objEstudiantes);
                listaEstudiantes.ItemsSource = _estu;
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Error al cargar: " + ex.Message, "OK");
        }
    }

    // ========== BÚSQUEDA ==========
    private void OnBuscarClicked(object sender, EventArgs e)
    {
        string textoBuscar = txtBuscar.Text?.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(textoBuscar))
        {
            // Si no hay texto, mostrar todos
            MostrarTodos();
            return;
        }

        // Filtrar estudiantes
        var filtrados = _estuOriginal.Where(estudiante =>
            estudiante.nombre.ToLower().Contains(textoBuscar) ||
            estudiante.apellido.ToLower().Contains(textoBuscar)
        ).ToList();

        if (filtrados.Count == 0)
        {
            DisplayAlert("Búsqueda", "No se encontraron estudiantes", "OK");
            MostrarTodos();
        }
        else
        {
            _estu = new ObservableCollection<Estudiante>(filtrados);
            listaEstudiantes.ItemsSource = _estu;
            lblSeleccionado.Text = $"🔍 Resultados: {filtrados.Count} estudiante(s) encontrado(s)";
        }
    }

    private void OnMostrarTodosClicked(object sender, EventArgs e)
    {
        MostrarTodos();
    }

    private void MostrarTodos()
    {
        if (_estuOriginal != null)
        {
            _estu = new ObservableCollection<Estudiante>(_estuOriginal);
            listaEstudiantes.ItemsSource = _estu;
            txtBuscar.Text = "";
            lblSeleccionado.Text = "👉 Selecciona un estudiante de la lista";
        }
    }

    // POST - Agregar estudiante
    private async void OnAgregarClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            await DisplayAlert("Error", "Ingresa el nombre", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(txtApellido.Text))
        {
            await DisplayAlert("Error", "Ingresa el apellido", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(txtEdad.Text))
        {
            await DisplayAlert("Error", "Ingresa la edad", "OK");
            return;
        }

        var nuevo = new Estudiante
        {
            nombre = txtNombre.Text,
            apellido = txtApellido.Text,
            edad = int.Parse(txtEdad.Text)
        };

        try
        {
            var json = JsonConvert.SerializeObject(nuevo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await cliente.PostAsync(URL, content);

            await DisplayAlert("Éxito", "Estudiante agregado", "OK");

            // Limpiar formulario
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtEdad.Text = "";

            await Task.Delay(500);
            Get(); // Recargar lista
            MostrarTodos(); // Mostrar todos después de recargar
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // PUT - Actualizar estudiante
    private async void OnActualizarClicked(object sender, EventArgs e)
    {
        if (_estudianteSeleccionado == null)
        {
            await DisplayAlert("Error", "Selecciona un estudiante de la lista", "OK");
            return;
        }

        string nuevoNombre = await DisplayPromptAsync("Actualizar", "Nuevo nombre:", initialValue: _estudianteSeleccionado.nombre);
        if (!string.IsNullOrEmpty(nuevoNombre))
        {
            string nuevoApellido = await DisplayPromptAsync("Actualizar", "Nuevo apellido:", initialValue: _estudianteSeleccionado.apellido);
            if (!string.IsNullOrEmpty(nuevoApellido))
            {
                string nuevaEdad = await DisplayPromptAsync("Actualizar", "Nueva edad:", initialValue: _estudianteSeleccionado.edad.ToString());
                if (!string.IsNullOrEmpty(nuevaEdad))
                {
                    _estudianteSeleccionado.nombre = nuevoNombre;
                    _estudianteSeleccionado.apellido = nuevoApellido;
                    _estudianteSeleccionado.edad = int.Parse(nuevaEdad);

                    try
                    {
                        var json = JsonConvert.SerializeObject(_estudianteSeleccionado);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var request = new HttpRequestMessage(HttpMethod.Put, URL);
                        request.Content = content;
                        await cliente.SendAsync(request);

                        await DisplayAlert("Éxito", "Estudiante actualizado", "OK");

                        await Task.Delay(500);
                        Get(); // Recargar lista
                        MostrarTodos();

                        lblSeleccionado.Text = "👉 Selecciona un estudiante de la lista";
                        _estudianteSeleccionado = null;
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", ex.Message, "OK");
                    }
                }
            }
        }
    }

    // DELETE - Eliminar estudiante
    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        if (_estudianteSeleccionado == null)
        {
            await DisplayAlert("Error", "Selecciona un estudiante de la lista", "OK");
            return;
        }

        bool confirmar = await DisplayAlert("Confirmar", $"¿Eliminar a {_estudianteSeleccionado.nombre}?", "SÍ", "NO");
        if (confirmar)
        {
            int codigoAEliminar = _estudianteSeleccionado.codigo;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, URL);
                var json = JsonConvert.SerializeObject(new { codigo = codigoAEliminar });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                await cliente.SendAsync(request);

                await DisplayAlert("Éxito", "Estudiante eliminado", "OK");

                await Task.Delay(500);
                Get(); // Recargar lista
                MostrarTodos();

                lblSeleccionado.Text = "👉 Selecciona un estudiante de la lista";
                _estudianteSeleccionado = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    // REFRESCAR
    private async void OnRefrescarClicked(object sender, EventArgs e)
    {
        lblSeleccionado.Text = "⏳ Cargando...";
        await Task.Delay(500);
        Get();
        MostrarTodos();
        lblSeleccionado.Text = "✅ Lista actualizada";
        await Task.Delay(1000);
        lblSeleccionado.Text = "👉 Selecciona un estudiante de la lista";
    }

    // Seleccionar estudiante
    private void OnEstudianteSeleccionado(object sender, SelectedItemChangedEventArgs e)
    {
        _estudianteSeleccionado = e.SelectedItem as Estudiante;
        if (_estudianteSeleccionado != null)
        {
            lblSeleccionado.Text = $"✅ Seleccionado: {_estudianteSeleccionado.nombre} {_estudianteSeleccionado.apellido} (ID: {_estudianteSeleccionado.codigo})";
        }
    }
}