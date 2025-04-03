namespace Business.Dtos;

public class StatusTypeResults<T> : StatusResults
{
    public T? Result { get; set; }
}

public class StatusTypeResults : StatusResults
{

}




