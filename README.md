# ODPNetConnect
A lib to odp.net

Usage:

    ODPNetConnect odp = new ODPNetConnect();
    if (!String.IsNullOrWhiteSpace(odp.ERROR))
    {
        throw new Exception(odp.ERROR);
    }
    string sql = @"INSERT INTO TABLE (D1, D2, D3)  VALUES (:D1, :D2, :D3)";

    Dictionary<string, object> parameters = new Dictionary<string, object>();
    parametros["D1"] = "D1";
    parametros["D2"] = "D2";
    parametros["D3"] = "D3";

    int affectedRows  = odp.ParameterizedWrite(sql, parameters);

    if (!String.IsNullOrWhiteSpace(odp.ERROR))
    {
        throw new Exception(odp.ERROR);
    }

