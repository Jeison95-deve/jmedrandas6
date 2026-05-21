using jmedrandas6.Modelos;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
namespace jmedrandas6.Views;

public partial class vEstudiante : ContentPage
{

	private const string URL = " http://10.2.1.21/ws_estudiante/post.php";
	private readonly HttpClient cliente = new HttpClient();
    private ObservableCollection<Estudiante> _estu;
	public async void Get()
	{
		var content = await cliente.GetStringAsync(URL);
		List<Estudiante> objEstudiantes = JsonConvert.DeserializeObject<List<Estudiante>>(content);
        _estu = new ObservableCollection<Estudiante>(objEstudiantes);
		listaEstudiantes.ItemsSource = _estu;
    }
    public vEstudiante()
	{
		InitializeComponent();
		Get();
    }
}