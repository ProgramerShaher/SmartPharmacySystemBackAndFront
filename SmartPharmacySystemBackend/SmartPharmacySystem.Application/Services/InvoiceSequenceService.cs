using AutoMapper;
using SmartPharmacySystem.Application.DTOs.InvoiceSequences;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class InvoiceSequenceService : IInvoiceSequenceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InvoiceSequenceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<InvoiceSequenceDto> GetNextNumberAsync(string prefix, int year)
    {
        var sequence = await _unitOfWork.InvoiceSequences.GetSequenceAsync(prefix, year);

        if (sequence == null)
        {
            sequence = new InvoiceNumberSequence
            {
                Prefix = prefix,
                Year = year,
                LastNumber = 0
            };
            await _unitOfWork.InvoiceSequences.AddAsync(sequence);
            await _unitOfWork.SaveChangesAsync();
        }

        sequence.LastNumber++;
        await _unitOfWork.InvoiceSequences.UpdateAsync(sequence);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<InvoiceSequenceDto>(sequence);
        dto.NextNumber = $"{prefix}-{year}-{sequence.LastNumber:D6}";
        return dto;
    }
}
