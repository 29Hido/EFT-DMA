using Vmmsharp;
using System.Runtime.InteropServices;
using System.Text;

namespace TarkovDMATest;

public static class Memory
{
    private const ulong PAGE_SIZE = 0x1000;
    
    public static VmmProcess _process;
    
    /// <summary>
    /// Resolves a pointer and returns the memory address it points to.
    /// </summary>
    public static ulong ReadPtr(ulong ptr)
    {
        var addr = ReadValue<ulong>(ptr);
        if (addr == 0x0) throw new NullPtrException();
        else return addr;
    }

    /// <summary>
    /// Resolves a pointer and returns the memory address it points to.
    /// </summary>
    public static ulong ReadPtrNullable(ulong ptr)
    {
        return ReadValue<ulong>(ptr);
    }

    /// <summary>
    /// Read value type/struct from specified address.
    /// </summary>
    /// <typeparam name="T">Specified Value Type.</typeparam>
    /// <param name="addr">Address to read from.</param>
    public static T ReadValue<T>(ulong addr) where T : struct
    {
        try
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            var buf = _process.MemRead(addr, (uint)size, Vmm.FLAG_NOCACHE);
            return MemoryMarshal.Read<T>(buf);
        }
        catch (Exception ex)
        {
            throw new DMAException($"ERROR reading {typeof(T)} value at 0x{addr:X}", ex);
        }
    }
    
    /// <summary>
    /// Read null terminated string.
    /// </summary>
    /// <param name="length">Number of bytes to read.</param>
    /// <exception cref="DMAException"></exception>
    public static string ReadString(ulong addr, uint length = 256)
    {
        try
        {
            if (length > PAGE_SIZE)
                throw new DMAException("String length outside expected bounds!");
            
            var buf = _process.MemRead(addr, length, Vmm.FLAG_NOCACHE);
            int nullTerminator = Array.IndexOf<byte>(buf, 0);

            return nullTerminator != -1
                ? Encoding.Default.GetString(buf, 0, nullTerminator)
                : Encoding.Default.GetString(buf);
        }
        catch (Exception ex)
        {
            throw new DMAException($"ERROR reading string at 0x{addr:X}", ex);
        }
    }
    
    /// <summary>
    /// Read a chain of pointers and get the final result.
    /// </summary>
    public static ulong ReadPtrChain(ulong ptr, uint[] offsets)
    {
        ulong addr = 0;
        try { addr = ReadPtr(ptr + offsets[0]); }
        catch (Exception ex) { throw new DMAException($"ERROR reading pointer chain at index 0, addr 0x{ptr:X} + 0x{offsets[0]:X}", ex); }
        for (int i = 1; i < offsets.Length; i++)
        {
            try { addr = ReadPtr(addr + offsets[i]); }
            catch (Exception ex) { throw new DMAException($"ERROR reading pointer chain at index {i}, addr 0x{addr:X} + 0x{offsets[i]:X}", ex); }
        }
        return addr;
    }
    
    public static void WriteValue<T>(ulong addr, T value)
        where T : unmanaged
    {
        try
        {
            if (!_process.MemWriteStruct(addr, value))
                throw new Exception("Memory Write Failed!");
        }
        catch (Exception ex)
        {
            throw new DMAException($"[DMA] ERROR writing {typeof(T)} value at 0x{addr.ToString("X")}", ex);
        }
    }
}

public class NullPtrException : Exception
{
    public NullPtrException()
    {
    }

    public NullPtrException(string message)
        : base(message)
    {
    }

    public NullPtrException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public class DMAException : Exception
{
    public DMAException()
    {
    }

    public DMAException(string message)
        : base(message)
    {
    }

    public DMAException(string message, Exception inner)
        : base(message, inner)
    {
    }
}