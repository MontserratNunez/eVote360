# eVote360 Pro

**eVote360 Pro** es una aplicación web de votación electrónica diseñada para gestionar de manera integral el ciclo de vida completo de un proceso electoral. Permite el registro y validación de ciudadanos elegibles (votantes), además de ofrecer módulos avanzados de gestión para la configuración de elecciones, partidos políticos, puestos electivos, candidatos y alianzas políticas.



## Capturas de Pantalla

### Vista Administrador

| Dashboard Admin | Partidos Políticos | Resumen de Elecciones |
| :---: | :---: | :---: |
| ![Dashboard Admin](https://github.com/user-attachments/assets/b4519b57-7f6c-4402-b996-9f01dfb45edf) | ![Partidos](https://github.com/user-attachments/assets/6149291d-b970-4433-96c8-e6fdb2fad80f) | ![Resumen Elecciones](https://github.com/user-attachments/assets/23e6739c-ceab-4e1a-a1ec-4789741706d2) |



### Vista Dirigente Político

| Dashboard Dirigente | Gestión de Candidatos | Alianzas Políticas |
| :---: | :---: | :---: |
| ![Dashboard Dirigente](https://github.com/user-attachments/assets/101b6181-1fa5-4f85-bfbc-8ed403fd1118) | ![Candidatos](https://github.com/user-attachments/assets/f41d72ec-75f2-4420-bbf6-3175aea66b29) | ![Alianzas Políticas](https://github.com/user-attachments/assets/5162d1ad-4a87-4d25-8856-26931273708d) |



### Vista del Votante

| Validación de Cédula (OCR) | Selección de Candidatos | Confirmación de Voto |
| :---: | :---: | :---: |
| ![Validación OCR](https://github.com/user-attachments/assets/d72b4c98-a44a-416a-b45a-ab3166b45cda) | ![Votar Candidato](https://github.com/user-attachments/assets/92fc24a7-3f74-4d9c-b9b7-1e2da9bbe9b2) | ![Enviar Votación](https://github.com/user-attachments/assets/70785125-2494-4267-b469-c1b1e4b25629) |



## Tecnologías Utilizadas
- **Framework:** ASP.NET Core MVC .NET 9
- **Arquitectura:** Onion Architecture
- **ORM:** Entity Framework Core
- **Base de Datos:** Microsoft SQL Server
- **Identidad y Seguridad:** Tesseract OCR, códigos de verificación por correo electrónico, Control de Acceso Basado en Roles
- **Frontend:** HTML5, CSS3, Bootstrap



## Pasos de Instalación

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/MontserratNunez/eVote360.git
   ```

2. **Configurar la Conexión a la Base de Datos:**
   Navega al directorio del proyecto web (`eVote360/`) y localiza el archivo `appsettings.json`. Actualiza la cadena de conexión `DefaultConnection` con tus credenciales de SQL Server:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=TU_NOMBRE_DE_SERVIDOR;Database=EVote360;Trusted_Connection=true;TrustServerCertificate=true"
   }
   ```

3. **Configurar el Servicio de Correo Electrónico:**
   En el mismo archivo `appsettings.json`, actualiza el bloque `EmailConfiguration` con tus credenciales SMTP para habilitar el envío de códigos de verificación y resúmenes de votación por correo:
   ```json
   "EmailConfiguration": {
     "Host": "smtp.gmail.com",
     "Port": 587,
     "Email": "tu_correo@gmail.com",
     "Password": "tu_contraseña_de_aplicacion"
   }
   ```
   *(Nota: Si utilizas Gmail, asegúrate de generar y usar una Contraseña de Aplicación en lugar de tu contraseña).*

4. **Aplicar las Migraciones de la Base de Datos:**
   Abre la Consola del Administrador de Paquetes o utiliza la CLI de .NET para aplicar las migraciones de EF Core y crear el esquema de la base de datos:
   ```bash
   dotnet ef database update --project Persistence --startup-project eVote360
   ```

5. **Usuario por Defecto:**

| Nombre de Usuario | Contraseña |
| :--- | :--- |
| Admin | 123 |



## Funcionalidades Principales

### Administrador
El Administrador cuenta con control total sobre la configuración electoral y el mantenimiento del sistema:
- **Dashboard y Resumen Electoral:** Consulta de resultados históricos de las elecciones filtrados por año, incluyendo los partidos participantes, candidatos y porcentaje de participación de votantes.
- **Gestión de Elecciones:** Creación, activación y finalización de elecciones. La modificación de datos críticos queda estrictamente bloqueada durante el transcurso de una elección activa.
- **Puestos Electivos:** Administración de los cargos públicos disponibles.
- **Padrón de Ciudadanos:** Mantenimiento de la base de datos de ciudadanos elegibles para votar.
- **Partidos Políticos:** Registro y gestión de organizaciones políticas, sus siglas y logotipos.
- **Gestión de Usuarios:** Creación de usuarios del sistema con asignación de roles (`Admin` o `Dirigente Político`).

### Dirigente Político
El Dirigente Político actúa en representación de un partido político específico y gestiona su estrategia electoral:
- **Gestión de Candidatos:** Registro de ciudadanos como candidatos asignados a su partido. La afiliación del candidato se maneja automáticamente según el contexto de la sesión del dirigente para evitar asignaciones no autorizadas.
- **Alianzas Políticas:** Envío, aceptación o rechazo de solicitudes de alianza con otros partidos políticos para el proceso electoral vigente.
- **Asignación de Candidatos a Puestos Electivos:** Asignación de candidatos activos a los puestos electivos en una elección pendiente. Los dirigentes pueden asignar a sus propios candidatos o a candidatos pertenecientes a partidos aliados.

### Votante
Los ciudadanos pueden ejercer su derecho al voto de manera intuitiva y segura:
- **Validación e Identificación OCR:** Verificación de la cédula de identidad mediante escaneo de imagen y reconocimiento óptico de caracteres (Tesseract OCR).
- **Proceso de Votación:** Selección interactiva de candidatos por puesto electivo con soporte para alianzas políticas.
- **Envío de Votación y Confirmación:** Envío seguro del sufragio con emisión y notificación de comprobante vía correo electrónico.



## Estructura de Carpetas
La solución está estructurada siguiendo los principios de Onion Architecture para garantizar la separación de responsabilidades:

- **`Domain/`**: La capa central (*Core*) que contiene la lógica del negocio principal, Entidades y Enumeraciones. No posee dependencias de otros proyectos.
- **`Application/`**: Contiene la lógica de negocio secundaria, Servicios, Interfaces, DTOs y ViewModels. Depende únicamente de la capa `Domain`.
- **`Persistence/`**: La capa de infraestructura para el acceso a datos. Contiene el `DbContext` de EF Core, Migraciones, Configuraciones de Entidades y Repositorios genéricos y específicos.
- **`eVote.Infraestructure.Shared/`**: Servicios de infraestructura compartidos, tales como el servicio de envío de correos electrónicos y la integración con Tesseract OCR.
- **`eVote360/`**: La capa de Presentación / Web. Contiene los Controladores MVC, Vistas, Middlewares y la configuración de inyección de dependencias.
