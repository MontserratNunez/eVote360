using eVote360.Core.Application.Common.Results;
using eVote360.Core.Application.Dtos.Vote;
using eVote360.Core.Application.Interfaces;
using eVote360.Core.Domain.Common.Enums;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Events;
using eVote360.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Core.Application.Services
{
    public class VoteService : IVoteService
    {
        private readonly ICitizenRepository _citizenRepository;
        private readonly IElectionRepository _electionRepository;
        private readonly IOcrService _ocrService;
        private readonly IVoteRepository _voteRepository;
        private readonly IVerificationCodeRepository _verificationCodeRepository;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IElectivePositionRepository _electivePositionRepository;
        private readonly IAssignPositionRepository _assignPositionRepository;

        public VoteService(
            ICitizenRepository citizenRepository, 
            IElectionRepository electionRepository,
            IOcrService ocrService,
            IVoteRepository voteRepository,
            IVerificationCodeRepository verificationCodeRepository,
            IEventDispatcher eventDispatcher,
            IElectivePositionRepository electivePositionRepository,
            IAssignPositionRepository assignPositionRepository)

        {
            _citizenRepository = citizenRepository;
            _electionRepository = electionRepository;
            _ocrService = ocrService;
            _voteRepository = voteRepository;
            _verificationCodeRepository = verificationCodeRepository;
            _eventDispatcher = eventDispatcher;
            _electivePositionRepository = electivePositionRepository;
            _assignPositionRepository = assignPositionRepository;
        }

        public async Task<Result<(int CitizenId, int ElectionId)>> StartAsync(StartVoteDto dto)
        {
            var result = new Result<(int, int)>();

            if (string.IsNullOrWhiteSpace(dto.DocumentNumber))
            {
                result.IsSuccess = false;
                result.Message = "Debe ingresar su número de documento.";
                return result;
            }

            var election = await _electionRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(e => e.Status == ElectionStatus.ACTIVE);

            if (election == null)
            {
                return new Result<(int, int)>
                {
                    IsSuccess = false,
                    Message = "No hay ningún proceso electoral en estos momentos."
                };
            }

            string documentNumber = FormatToOfficialDocument(dto.DocumentNumber);

            var citizen = await _citizenRepository
                .GetAllQuery()
                .FirstOrDefaultAsync(c => c.DocumentNumber == documentNumber);

            if (citizen == null)
            {
                return new Result<(int, int)>
                {
                    IsSuccess = false,
                    Message = "No existe un ciudadano registrado con este número de documento."
                };
            }

            if (!citizen.Status)
            {
                return new Result<(int, int)>
                {
                    IsSuccess = false,
                    Message = "Este ciudadano se encuentra inactivo y no puede participar en el proceso de votación."
                };
            }

            bool hasVoted = await _voteRepository
                .GetAllQuery()
                .AnyAsync(v =>
                    v.CitizenId == citizen.Id &&
                    v.ElectionId == election.Id);

            if (hasVoted)
            {
                return new Result<(int, int)>
                {
                    IsSuccess = false,
                    Message = "Ya ha ejercido su derecho al voto."
                };
            }

            result.IsSuccess = true;
            result.Data = (citizen.Id, election.Id);

            return result;
        }

        private string FormatToOfficialDocument(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            string digits = new string(input.Where(char.IsDigit).ToArray());

            if (digits.Length != 11) return input.Trim();

            string area = digits.Substring(0, 3);
            string sequence = digits.Substring(3, 7);
            string verifier = digits.Substring(10, 1);

            return $"{area}-{sequence}-{verifier}";
        }

        public async Task<Result> ValidateIdentityAsync(OcrValidationDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Debe subir una imagen de su cédula para validar su identidad."
                };
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            var extension = Path.GetExtension(dto.File.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El archivo seleccionado no tiene un formato de imagen válido."
                };
            }



            var extractedText = await _ocrService.ExtractTextAsync(dto.File);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No fue posible leer correctamente el número de documento en la imagen cargada. Por favor, suba una imagen más clara."
                };
            }

            var citizen = await _citizenRepository.GetById(dto.CitizenId);

            if (citizen == null)
            {
                return new Result { IsSuccess = false, Message = "El ciudadano no existe." };
            }

            /*
            if (!extractedText.Contains(citizen.DocumentNumber))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Los datos extraídos de la foto no coinciden con los datos previamente ingresados por el elector."
                };
            }
            */

            Console.WriteLine(extractedText);

            string cleanOcrText = new string(extractedText.Where(char.IsDigit).ToArray());

            string cleanDbDocument = new string(citizen.DocumentNumber.Where(char.IsDigit).ToArray());

            Console.WriteLine(cleanOcrText);

            Console.WriteLine(cleanDbDocument);

            if (!cleanOcrText.Contains(cleanDbDocument))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Los datos extraídos de la foto no coinciden con los datos previamente ingresados por el elector. Por favor, suba una imagen más clara."
                };
            }


            if (string.IsNullOrWhiteSpace(citizen.Email))
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este ciudadano no tiene un correo electrónico registrado. No es posible continuar con la verificación de identidad."
                };
            }

            

            var random = new Random();
            string generatedCode = random.Next(100000, 999999).ToString();

            var verificationEntity = new VerificationCode
            {
                CitizenId = citizen.Id,
                ElectionId = dto.ElectionId,
                Code = generatedCode,
                CreatedDate = DateTime.Now,
                ExpirationDate = DateTime.Now.AddMinutes(5),
                Used = false
            };

            try
            {
                await _verificationCodeRepository.AddAsync(verificationEntity);
            }
            catch (Exception)
            {
                return new Result { IsSuccess = false, Message = "Error interno al procesar el código de seguridad." };
            }

            
            try
            {
                var emailEvent = new ConfirmationCodeEvent
                {
                    Code = generatedCode,
                    Email = citizen.Email,
                    Name = $"{citizen.Name} {citizen.LastName}"
                };

                await _eventDispatcher.DispatchAsync(emailEvent);
            }
            catch (Exception)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No fue posible enviar el código de verificación. Intente nuevamente más tarde."
                };
            }

            return new Result
            {
                IsSuccess = true,
                Message = "Identidad validada correctamente. Se ha enviado un código de verificación a su correo electrónico."
            };
        }

        public async Task<Result> VerifyCodeAsync(VerifyCodeDto dto)
        {
            var lastCodeRecord = await _verificationCodeRepository.GetAllQuery()
                .Where(v => v.CitizenId == dto.CitizenId && v.ElectionId == dto.ElectionId)
                .OrderByDescending(v => v.CreatedDate)
                .FirstOrDefaultAsync();

            if (lastCodeRecord == null)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El código de verificación ingresado no es válido."
                };
            }

            if (lastCodeRecord.Code != dto.Code)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El código de verificación ingresado no es válido."
                };
            }

            if (lastCodeRecord.Used)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "Este código de verificación ya fue utilizado."
                };
            }

            if (DateTime.Now > lastCodeRecord.ExpirationDate)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "El código de verificación ha expirado. Solicite un nuevo código para continuar."
                };
            }

            lastCodeRecord.Used = true;

            await _verificationCodeRepository.UpdateAsync(lastCodeRecord.Id, lastCodeRecord);

            return new Result
            {
                IsSuccess = true,
                Message = "Código verificado exitosamente."
            };
        }

        public async Task<Result<List<AvailablePositionsDto>>> GetAvailablePositionsAsync(Dictionary<int, int?> selectedVotes)
        {
            var result = new Result<List<AvailablePositionsDto>>();

            try
            {
                var activePositions = await _electivePositionRepository.GetAllQuery()
                    .Where(p => p.Status == true)
                    .ToListAsync();

                var allAssignments = await _assignPositionRepository.GetAllQuery()
                    .ToListAsync();

                var positionsDtoList = new List<AvailablePositionsDto>();

                foreach (var position in activePositions)
                {
                    var positionAssignments = allAssignments
                        .Where(a => a.ElectivePositionId == position.Id)
                        .ToList();

                    int totalParties = positionAssignments.Count;

                    int totalRealCandidates = positionAssignments
                        .Select(a => a.CandidateId)
                        .Distinct()
                        .Count();

                    bool isSelected = selectedVotes.ContainsKey(position.Id) && selectedVotes[position.Id].HasValue;

                    positionsDtoList.Add(new AvailablePositionsDto
                    {
                        Id = position.Id,
                        PositionName = position.Name,
                        TotalParties = totalParties,
                        TotalRealCandidates = totalRealCandidates,
                        IsSelected = isSelected
                    });
                }

                result.IsSuccess = true;
                result.Data = positionsDtoList;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al calcular los puestos electivos disponibles.";
            }

            return result;
        }

        public async Task<Result<CandidatesDto>> GetCandidatesByPositionAsync(int positionId)
        {
            var result = new Result<CandidatesDto>();

            try
            {
                var position = await _electivePositionRepository.GetById(positionId);
                if (position == null || !position.Status)
                {
                    result.IsSuccess = false;
                    result.Message = "El puesto electivo seleccionado no está disponible o no existe.";
                    return result;
                }

                var assignments = await _assignPositionRepository.GetAllQuery()
                    .Include(a => a.Candidate)
                    .Include(a => a.PoliticalParty)
                    .Where(a => a.ElectivePositionId == positionId && a.Candidate.Status == true)
                    .ToListAsync();

                var options = assignments.Select(a => new CandidateOptionDto
                {
                    AssignPositionId = a.Id,
                    CandidateId = a.CandidateId,
                    CandidateFullName = $"{a.Candidate.Name} {a.Candidate.LastName}",
                    CandidatePhotoPath = a.Candidate.PhotoPath,
                    PoliticalPartyId = a.PoliticalPartyId,
                    PoliticalPartyName = a.PoliticalParty.Name,
                    PoliticalPartyLogoPath = a.PoliticalParty.LogoPath
                }).ToList();

                result.IsSuccess = true;
                result.Data = new CandidatesDto
                {
                    PositionId = positionId,
                    PositionName = position.Name,
                    Candidates = options
                };
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al recuperar la lista de candidatos para el puesto.";
            }

            return result;
        }

        public async Task<Result> FinalizeVotingAsync(FinalizeVoteDto dto)
        {
            var election = await _electionRepository.GetById(dto.ElectionId);
            if (election == null || election.Status != ElectionStatus.ACTIVE)
            {
                return new Result { IsSuccess = false, Message = "No existe una elección activa en este momento." };
            }

            var citizen = await _citizenRepository.GetById(dto.CitizenId);
            if (citizen == null || !citizen.Status)
            {
                return new Result { IsSuccess = false, Message = "El ciudadano no se encuentra activo para participar." };
            }

            var alreadyVoted = await _voteRepository.GetAllQuery()
                .AnyAsync(ce => ce.CitizenId == dto.CitizenId && ce.ElectionId == dto.ElectionId /*  && ce.HasVoted == true*/);

            if (alreadyVoted)
            {
                return new Result { IsSuccess = false, Message = "Ya ha ejercido su derecho al voto en esta elección." };
            }

            var activePositions = await _electivePositionRepository.GetAllQuery()
                .Where(p => p.Status == true)
                .ToListAsync();

            var missingPositions = new List<string>();
            foreach (var position in activePositions)
            {
                if (!dto.SelectedVotes.ContainsKey(position.Id) || dto.SelectedVotes[position.Id] == null)
                {
                    missingPositions.Add(position.Name);
                }
            }

            if (missingPositions.Count > 0)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = $"Debe completar su selección para los siguientes puestos electivos: {string.Join(", ", missingPositions)}."
                };
            }

            var eventLines = new List<VoteSummaryLine>();

            foreach (var position in activePositions)
            {
                int selectionId = dto.SelectedVotes[position.Id]!.Value;

                if (selectionId == -1)
                {
                    eventLines.Add(new VoteSummaryLine
                    {
                        PositionName = position.Name,
                        Selection = "Ninguno",
                        PoliticalParty = string.Empty
                    });

                    await _voteRepository.AddAsync(new Vote
                    {
                        CitizenId = citizen.Id,
                        ElectionId = dto.ElectionId,
                        ElectivePositionId = position.Id,
                        CandidateId = null,
                        CreatedDate = DateTime.Today,
                        IsBlank = true
                    });
                }
                else
                {
                    var assignment = await _assignPositionRepository.GetAllQuery()
                        .Include(a => a.Candidate)
                        .Include(a => a.PoliticalParty)
                        .FirstOrDefaultAsync(a => a.Id == selectionId);

                    if (assignment != null)
                    {
                        eventLines.Add(new VoteSummaryLine
                        {
                            PositionName = position.Name,
                            Selection = $"{assignment.Candidate.Name} {assignment.Candidate.LastName}",
                            PoliticalParty = assignment.PoliticalParty.Name
                        });

                        await _voteRepository.AddAsync(new Vote
                        {
                            CitizenId = citizen.Id,
                            ElectionId = dto.ElectionId,
                            ElectivePositionId = position.Id,
                            CandidateId = assignment.Candidate.Id,
                            CreatedDate = DateTime.Today,
                            IsBlank = false
                        });
                    }
                }
            }

            try
            {
                var summaryEvent = new VoteSummaryEvent
                {
                    CitizenName = $"{citizen.Name} {citizen.LastName}",
                    CitizenEmail = citizen.Email,
                    ElectionName = election.Name,
                    ElectionDate = election.ActivatedDate.Value.ToString("dd/MM/yyyy"),
                    VotedPositions = eventLines
                };

                await _eventDispatcher.DispatchAsync(summaryEvent);
            }
            catch (Exception)
            {
                return new Result
                {
                    IsSuccess = false,
                    Message = "No fue posible enviar el resumen de votación."
                };
            }

            return new Result { IsSuccess = true, Message = "Votación finalizada con éxito." };
        }
    }
}
