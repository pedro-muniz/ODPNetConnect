# ODPNetConnect
A lib to odp.net

Usage:

    ODPNetConnect odp = new ODPNetConnect();
    if (!String.IsNullOrWhiteSpace(odp.ERROR))
    {
        throw new Exception(odp.ERROR);
    }
    string sql = @"INSERT INTO TABLE (D1, D2, D3)  VALUES (:D1, :D2, :D3)";

    Dictionary<string, object> params = new Dictionary<string, object>();
    params["D1"] = "D1";
    params["D2"] = "D2";
    params["D3"] = "D3";

    int affectedRows  = odp.ParameterizedWrite(sql, parameters);

    if (!String.IsNullOrWhiteSpace(odp.ERROR))
    {
        throw new Exception(odp.ERROR);
    }

