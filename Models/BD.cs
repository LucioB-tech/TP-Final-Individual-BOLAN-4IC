
using Microsoft.Data.SqlClient;

namespace TP_Final_Lucio.Models
{
    public class BD
    {
        private string conexion;

        public BD(IConfiguration configuracion)
        {
            conexion = configuracion.GetConnectionString("ConexionSQL");
        }

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(conexion);
        }
    }
}
