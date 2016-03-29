# ODPNetConnect
A lib to odp.net
so you can do parameterized write and read like this:

    ODPNetConnect odp = new ODPNetConnect();
    if (!String.IsNullOrWhiteSpace(odp.ERROR))
    {
        throw new Exception(odp.ERROR);
    }
     
    //Write:
    string sql = @"INSERT INTO TABLE (D1, D2, D3)  VALUES (:D1, :D2, :D3)";
    
    Dictionary<string, object> params = new Dictionary<string, object>();
    params["D1"] = "D1";
    params["D2"] = "D2";
    params["D3"] = "D3";
    
    int affectedRows  = odp.ParameterizedWrite(sql, params);
    
    if (!String.IsNullOrWhiteSpace(odp.ERROR))
    {
        throw new Exception(odp.ERROR);
    }

    //read
    string sql = @"SELECT * FROM TABLE WHERE D1 = :D1";
    
    Dictionary<string, object> params = new Dictionary<string, object>();
    params["D1"] = "D1";
    
    DataTable dt = odp.ParameterizedRead(sql, params);
    if (!String.IsNullOrWhiteSpace(odp.ERROR))
    {
        throw new Exception(odp.ERROR);
    }

Notes: you have to change these lines in ODPNetConnect.cs to set connection string:

    static private string devConnectionString = "SET YOUR DEV CONNECTION STRING";
    static private string productionConnectionString = "SET YOUR PRODUCTION CONNECTION STRING";

And you need to change line 123 to set environment to dev or prod.

    public OracleConnection GetConnection(string env = "dev", bool cacheOn = false)
